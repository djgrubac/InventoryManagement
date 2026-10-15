-- =============================================================================
-- Inventory Management - production-like seed data (PostgreSQL 13+)
--
-- Models an industrial & construction supply distributor with four divisions:
--   HW  Hardware & building supplies   (fasteners, tools, paint, plumbing, electrical, PPE)
--   BLK Bulk building materials        (cement, aggregates, blocks, insulation, timber
--                                       sheets, roofing, steel, liquids in drums / IBCs)
--   AUT Automotive & spare parts       (filters, brakes, oils, tyres, batteries, truck parts)
--   IND Industrial supplies            (bearings, hydraulics, pneumatics, welding, lifting)
-- Divisions can be switched off in section 0.
--
-- Size presets (pick with -v size=small|medium|large, default medium):
--   small   ~600 products,   5 warehouses, 6 months  -> ~5k orders,   ~20k items     (seconds)
--   medium  ~1,500 products, 8 warehouses, 1 year    -> ~30k orders,  ~150k items    (~10 s)
--   large   ~6,000 products, 15 warehouses, 3 years  -> ~280k orders, ~1.3M items    (~1 min, ~750 MB)
--
-- Business rules (same as OrderService / ChangeOrderStatusCommandHandler):
--   * One line per product per order; item Price = product price when the order
--     was placed (older orders carry lower prices - ~6% yearly price increase).
--   * Order.TotalPrice = SUM(item.Price * item.Quantity); no empty Completed orders.
--   * Only Completed orders move stock: Incoming adds, Outgoing subtracts.
--     Cancelled orders keep their items and never touch stock.
--   * Stock never goes negative at any point in time: every outgoing shipment is
--     covered by incoming deliveries that were completed BEFORE it
--     (replenishment is derived from the demand, with 1-12 days lead time).
--   * Stocks.Quantity = existing quantity + SUM(completed incoming) - SUM(completed outgoing).
--
-- Existing rows are reused and never modified, except Stocks rows that receive
-- new movements. Re-running the script does nothing once it has been applied.
--
-- Run:
--   psql -h localhost -p 5433 -U postgres -d inventory_management -f scripts/seed-dummy-data.sql
--   psql ... -v size=small -f scripts/seed-dummy-data.sql
-- Then check every invariant:
--   psql -h localhost -p 5433 -U postgres -d inventory_management -f scripts/verify-seed-data.sql
-- =============================================================================

\set ON_ERROR_STOP on
\timing on

-- ---- size preset & tunables -------------------------------------------------
\if :{?size}
\else
    \set size medium
\endif

SELECT :'size' IN ('small', 'medium', 'large') AS size_ok \gset
\if :size_ok
\else
    \echo 'Unknown size' :'size' '- use small, medium or large.'
    \quit
\endif

SELECT CASE :'size' WHEN 'small' THEN '2026-04-06' WHEN 'medium' THEN '2025-10-06' ELSE '2023-10-09' END AS history_start,
       -- outgoing orders per day per unit of warehouse size (sizes are in section 3)
       CASE :'size' WHEN 'small' THEN 4    WHEN 'medium' THEN 8    ELSE 15  END AS orders_per_day,
       -- share of the full ~6,000-product catalog to keep
       CASE :'size' WHEN 'small' THEN 0.10 WHEN 'medium' THEN 0.25 ELSE 1.0 END AS catalog_share,
       -- how many of the 13 new warehouses to create (existing ones are always used)
       CASE :'size' WHEN 'small' THEN 3    WHEN 'medium' THEN 6    ELSE 13  END AS new_warehouses
\gset

\set history_end    '2026-10-07'
\set now_ts         '2026-10-08 08:00'
\set random_seed    0.2026
\echo 'Seeding size' :'size' 'from' :'history_start' 'to' :'history_end'
-- -----------------------------------------------------------------------------

SELECT EXISTS (SELECT 1 FROM "Categories" WHERE "Caption" = 'Fasteners') AS already_seeded \gset
\if :already_seeded
    \echo 'Seed data already present (category "Fasteners" exists) - nothing to do.'
    \quit
\endif

BEGIN;
SET LOCAL timezone = 'Europe/Belgrade';
SET LOCAL work_mem = '256MB';
SELECT setseed(:random_seed);

CREATE TEMP SEQUENCE seed_ok;  -- temporary order keys

CREATE TEMP TABLE seed_users ON COMMIT DROP AS
SELECT array_agg("Id" ORDER BY "UserName") AS ids FROM "AspNetUsers";

-- =============================================================================
-- 0. Divisions (set enabled = false to leave a division out)
-- =============================================================================
CREATE TEMP TABLE seed_division (code text PRIMARY KEY, name text, enabled boolean, lines_per_order numeric) ON COMMIT DROP;
INSERT INTO seed_division VALUES
    ('HW',  'Hardware & building supplies', true, 4.0),
    ('BLK', 'Bulk building materials',      true, 1.5),
    ('AUT', 'Automotive & spare parts',     true, 2.5),
    ('IND', 'Industrial supplies',          true, 3.0);

-- =============================================================================
-- 1. Categories
-- =============================================================================
CREATE TEMP TABLE seed_category (code text PRIMARY KEY, division text, caption text, id int) ON COMMIT DROP;
INSERT INTO seed_category (code, division, caption) VALUES
    ('FAS','HW','Fasteners'), ('HTL','HW','Hand Tools'), ('PTL','HW','Power Tools'),
    ('ACC','HW','Power Tool Accessories'), ('PNT','HW','Paints & Coatings'), ('PLB','HW','Plumbing'),
    ('ELC','HW','Electrical'), ('PPE','HW','Safety Equipment (PPE)'),
    ('CEM','BLK','Cement & Mortars'), ('AGG','BLK','Aggregates'), ('BRK','BLK','Bricks, Blocks & Paving'),
    ('INS','BLK','Insulation'), ('DRY','BLK','Drywall Systems'), ('TMB','BLK','Timber & Boards'),
    ('ROF','BLK','Roofing & Drainage'), ('STL','BLK','Reinforcement & Steel'), ('LIQ','BLK','Bulk Liquids & Containers'),
    ('FLT','AUT','Filters'), ('BRA','AUT','Brakes'), ('SUS','AUT','Suspension'),
    ('ENG','AUT','Engine & Transmission Parts'), ('LGT','AUT','Lighting & Wipers'), ('BAT','AUT','Batteries'),
    ('OIL','AUT','Oils & Fluids'), ('TYR','AUT','Tyres'), ('TRK','AUT','Truck Parts'),
    ('BRG','IND','Bearings'), ('PTR','IND','Power Transmission'), ('HYD','IND','Hydraulics'),
    ('PNE','IND','Pneumatics'), ('WLD','IND','Welding'), ('LUB','IND','Industrial Lubricants'),
    ('LFT','IND','Lifting & Load Securing'), ('LOG','IND','Packaging & Logistics');

DELETE FROM seed_category c USING seed_division d WHERE d.code = c.division AND NOT d.enabled;

INSERT INTO "Categories" ("Uid", "Caption", "Created", "CreatedBy", "LastModified", "LastModifiedBy")
SELECT gen_random_uuid(), c.caption, t, u.ids[1], t, u.ids[1]
  FROM seed_category c, seed_users u, (SELECT :'history_start'::timestamptz - interval '45 days' AS t) x
 WHERE NOT EXISTS (SELECT 1 FROM "Categories" e WHERE e."Caption" = c.caption)
 ORDER BY c.division, c.code;

UPDATE seed_category c SET id = e."Id" FROM "Categories" e WHERE e."Caption" = c.caption;

-- =============================================================================
-- 2. Product catalog: families x variants (d1 x d2 x d3)
--    price = base * f1^(i1-1) * f2^(i2-1) * f3^(i3-1)   (m1/m2/m3 override f per variant)
--    qmin/qmax = typical quantity per customer order line, pop = demand weight
-- =============================================================================
CREATE TEMP TABLE seed_list (k text PRIMARY KEY, v text[]) ON COMMIT DROP;
INSERT INTO seed_list VALUES
('cars', ARRAY['VW Golf VII 1.6 TDI','VW Passat B8 2.0 TDI','VW Polo 1.0 TSI','Skoda Octavia III 1.6 TDI','Skoda Fabia III 1.0 MPI',
               'Audi A4 B9 2.0 TDI','Audi A3 8V 1.5 TFSI','BMW 320d F30','BMW 520d G30','Mercedes C220d W205',
               'Mercedes Sprinter 316 CDI','Opel Astra K 1.6 CDTI','Opel Corsa E 1.4','Ford Focus III 1.5 TDCi','Ford Transit 2.0 EcoBlue',
               'Renault Clio IV 1.5 dCi','Renault Megane IV 1.5 dCi','Dacia Duster 1.5 dCi','Peugeot 308 1.6 BlueHDi','Citroen Berlingo 1.6 HDi',
               'Fiat Punto 1.3 Multijet','Fiat Doblo 1.6 Multijet','Toyota Corolla 1.8 Hybrid','Toyota Yaris 1.5','Hyundai i30 1.6 CRDi',
               'Kia Sportage 1.7 CRDi','Nissan Qashqai 1.5 dCi','Mazda 3 2.0 Skyactiv','Seat Leon 1.6 TDI','Iveco Daily 2.3']),
('trucks', ARRAY['MAN TGX 18.440','Mercedes Actros 1845','Volvo FH 500','Scania R450','DAF XF 480',
                 'Iveco Stralis 460','Renault T 480','MAN TGS 26.400','Mercedes Atego 1224','Iveco Daily 35S16']),
('colors', ARRAY['White','Off-White','Ivory','Light Grey','Anthracite','Sand Beige','Terracotta','Sky Blue','Mint Green','Lemon Yellow','Pastel Pink','Black']),
('brands', ARRAY['Voltmaster','Ironcraft','Pro-Tek']),
('ral',    ARRAY['RAL 3009 Red','RAL 8017 Brown','RAL 7016 Anthracite','RAL 9002 White','RAL 6020 Green','Unpainted']);

CREATE TEMP TABLE seed_family (
    fid serial, cat text, family text, d1 text[], d2 text[], d3 text[],
    base numeric, f1 numeric, f2 numeric, f3 numeric, unit text, qmin int, qmax int, pop numeric,
    m1 numeric[], m2 numeric[], m3 numeric[]
) ON COMMIT DROP;

INSERT INTO seed_family (cat, family, d1, d2, d3, base, f1, f2, f3, unit, qmin, qmax, pop) VALUES
-- ---------------------------------------------------------------- HW: Fasteners
('FAS','Wood Screw', ARRAY['3x16','3x20','3.5x25','3.5x30','4x30','4x40','4x50','4.5x50','4.5x60','5x50','5x60','5x70','5x80','6x80','6x100','6x120'], ARRAY['Zinc Plated','Yellow Zinc','Stainless A2'], ARRAY['Box 200','Box 500'], 3.9,1.12,1.5,2.3,'box',1,40,2.0),
('FAS','Drywall Screw', ARRAY['3.5x25','3.5x35','3.5x45','3.9x55','3.9x65'], ARRAY['Phosphated','Zinc Plated'], ARRAY['Box 1000','Bucket 5000'], 6.5,1.15,1.1,4.2,'box',1,30,2.0),
('FAS','Hex Bolt DIN 933', ARRAY['M6','M8','M10','M12','M14','M16','M20'], ARRAY['x20','x30','x40','x50','x60','x80','x100','x120'], ARRAY['8.8 Zinc','10.9 Black','A2 Stainless'], 4.0,1.35,1.12,1.6,'pack of 50',1,25,1.2),
('FAS','Hex Nut DIN 934', ARRAY['M4','M5','M6','M8','M10','M12','M14','M16','M20','M24'], ARRAY['Zinc Plated','A2 Stainless','Hot-Dip Galvanized'], NULL, 2.2,1.3,1.6,1,'pack of 100',1,30,1.5),
('FAS','Flat Washer DIN 125', ARRAY['M4','M5','M6','M8','M10','M12','M14','M16','M20','M24'], ARRAY['Zinc Plated','A2 Stainless','Hot-Dip Galvanized'], NULL, 1.5,1.3,1.6,1,'pack of 100',1,30,1.3),
('FAS','Spring Washer DIN 127', ARRAY['M4','M5','M6','M8','M10','M12','M14','M16','M20','M24'], ARRAY['Zinc Plated','A2 Stainless'], NULL, 1.8,1.3,1.7,1,'pack of 100',1,20,0.8),
('FAS','Threaded Rod DIN 975', ARRAY['M6','M8','M10','M12','M14','M16','M20'], ARRAY['1m','2m','3m'], ARRAY['Zinc Plated','A2 Stainless'], 1.4,1.35,1.9,2.6,'piece',2,60,1.2),
('FAS','Nylon Wall Plug', ARRAY['5x25','6x30','8x40','10x50','12x60','14x70'], ARRAY['Box 100','Box 500'], NULL, 2.1,1.3,4.2,1,'box',1,40,1.8),
('FAS','Steel Wedge Anchor', ARRAY['M8x75','M8x95','M10x90','M10x110','M12x100','M12x120','M16x125','M16x145'], ARRAY['Zinc Plated','A4 Stainless'], ARRAY['Box 25','Box 50'], 9.0,1.18,2.2,1.85,'box',1,20,1.0),
('FAS','Self-Drilling Roofing Screw', ARRAY['5.5x19','5.5x25','5.5x32','6.3x50','6.3x70','6.3x100'], (SELECT v FROM seed_list WHERE k='ral'), ARRAY['Box 100','Box 250'], 7.0,1.15,1.0,2.3,'box',1,30,1.0),
('FAS','Chipboard Screw Torx', ARRAY['3x16','3x20','3.5x30','4x35','4x40','4.5x45','5x50','5x60','5x70','6x80','6x100','6x140','6x200','8x200','8x300'], ARRAY['Yellow Zinc','Zinc Plated'], ARRAY['Box 100','Box 500'], 2.4,1.14,1.0,4.0,'box',1,40,1.6),
('FAS','Blind Rivet', ARRAY['3.2x8','3.2x10','4x8','4x10','4x12','4.8x12','4.8x16','4.8x20'], ARRAY['Aluminium/Steel','Stainless A2','Aluminium/Aluminium'], ARRAY['Box 500'], 8.0,1.1,1.8,1,'box',1,15,0.7),
-- ---------------------------------------------------------------- HW: Hand tools
('HTL','Combination Spanner', ARRAY['6mm','7mm','8mm','9mm','10mm','11mm','12mm','13mm','14mm','15mm','16mm','17mm','18mm','19mm','21mm','22mm','24mm','27mm','30mm','32mm'], ARRAY['Standard','Ratcheting'], NULL, 4.5,1.07,2.8,1,'piece',1,10,0.8),
('HTL','Socket', ARRAY['1/4" Drive','3/8" Drive','1/2" Drive'], ARRAY['8mm','10mm','12mm','13mm','14mm','15mm','17mm','19mm','22mm','24mm','27mm','30mm','32mm'], ARRAY['Hex','Impact'], 2.2,1.3,1.06,1.8,'piece',1,10,0.6),
('HTL','Screwdriver', ARRAY['Slotted 3x75','Slotted 4x100','Slotted 5.5x125','Slotted 6.5x150','Phillips PH0','Phillips PH1','Phillips PH2','Phillips PH3','Pozidriv PZ1','Pozidriv PZ2','Pozidriv PZ3','Torx T10','Torx T15','Torx T20','Torx T25','Torx T30'], ARRAY['Standard','VDE 1000V Insulated'], NULL, 3.8,1.02,2.1,1,'piece',1,12,0.9),
('HTL','Pliers', ARRAY['Combination','Long Nose','Diagonal Cutter','Water Pump','Locking','Wire Stripper','Circlip Internal','Circlip External'], ARRAY['160mm','180mm','200mm','250mm'], ARRAY['Standard','VDE 1000V'], 9.0,1.05,1.1,1.8,'piece',1,8,0.7),
('HTL','Claw Hammer', ARRAY['300g','450g','560g','680g'], ARRAY['Steel Handle','Fibreglass Handle','Wooden Handle'], NULL, 11.0,1.15,0.85,1,'piece',1,10,0.8),
('HTL','Machinist Hammer', ARRAY['300g','500g','800g','1000g','1500g'], ARRAY['Fibreglass Handle','Wooden Handle'], NULL, 8.0,1.2,0.85,1,'piece',1,8,0.5),
('HTL','Sledge Hammer', ARRAY['2kg','3kg','5kg','8kg'], ARRAY['Fibreglass Handle','Wooden Handle'], NULL, 18.0,1.3,0.85,1,'piece',1,6,0.4),
('HTL','Rubber Mallet', ARRAY['340g','450g','680g','900g'], ARRAY['Black','White Non-Marking'], NULL, 6.0,1.15,1.3,1,'piece',1,10,0.5),
('HTL','Spirit Level', ARRAY['40cm','60cm','80cm','100cm','120cm','150cm','180cm','200cm'], ARRAY['Aluminium','Magnetic Aluminium','Heavy Duty Box'], NULL, 9.0,1.15,1.3,1,'piece',1,8,0.7),
('HTL','Tape Measure', ARRAY['3m','5m','8m','10m'], ARRAY['16mm','19mm','25mm'], ARRAY['Standard','Magnetic Hook'], 4.0,1.3,1.15,1.25,'piece',1,20,1.2),
('HTL','Hand Saw', ARRAY['Wood 400mm','Wood 500mm','Wood 550mm','Hacksaw 300mm','Drywall 150mm','Aerated Concrete 700mm','Pruning 250mm','Japanese Pull 240mm'], NULL, NULL, 12.0,1.06,1,1,'piece',1,8,0.5),
('HTL','Wood Chisel', ARRAY['6mm','8mm','10mm','12mm','16mm','18mm','20mm','25mm','32mm','38mm'], ARRAY['Standard','Bevel Edge Pro'], NULL, 6.0,1.06,1.9,1,'piece',1,6,0.3),
('HTL','Utility Knife', ARRAY['9mm Snap-Off','18mm Snap-Off','25mm Snap-Off','Trapezoid Blade'], ARRAY['Plastic','Metal','Auto-Lock'], NULL, 2.5,1.2,1.6,1,'piece',1,25,1.2),
('HTL','Spare Blades', ARRAY['9mm','18mm','25mm','Trapezoid'], ARRAY['Pack 10','Dispenser 50'], NULL, 1.8,1.2,4.0,1,'pack',1,40,1.5),
('HTL','Hex Key', ARRAY['1.5mm','2mm','2.5mm','3mm','4mm','5mm','6mm','8mm','10mm','12mm','14mm'], ARRAY['L-Type','Ball End','T-Handle'], NULL, 0.8,1.18,1.6,1,'piece',1,20,0.4),
('HTL','Tool Box', ARRAY['16"','19"','22"','26"'], ARRAY['Plastic','Metal','Rolling'], NULL, 12.0,1.25,1.6,1,'piece',1,5,0.4),
('HTL','F-Clamp', ARRAY['150mm','200mm','300mm','400mm','500mm','600mm','800mm','1000mm'], ARRAY['80mm Reach','120mm Reach'], NULL, 6.0,1.1,1.3,1,'piece',1,12,0.5),
('HTL','Quick Grip Clamp', ARRAY['150mm','300mm','450mm','600mm','900mm'], NULL, NULL, 9.0,1.15,1,1,'piece',1,10,0.4),
-- ---------------------------------------------------------------- HW: Power tools
('PTL','Cordless Drill Driver', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['12V','18V'], ARRAY['Bare Tool','Kit 2x2.0Ah','Kit 2x5.0Ah'], 69,1.25,1.35,1.9,'piece',1,5,0.8),
('PTL','Cordless Impact Driver', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['18V'], ARRAY['Bare Tool','Kit 2x2.0Ah','Kit 2x5.0Ah'], 89,1.25,1,1.9,'piece',1,4,0.6),
('PTL','Cordless Combi Drill', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['18V','36V'], ARRAY['Bare Tool','Kit 2x2.0Ah','Kit 2x5.0Ah'], 99,1.25,1.6,1.9,'piece',1,4,0.6),
('PTL','Rotary Hammer', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['SDS-plus 800W','SDS-plus 1100W','SDS-max 1500W','SDS-max 1700W'], NULL, 119,1.25,1.4,1,'piece',1,3,0.5),
('PTL','Angle Grinder', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['115mm 750W','125mm 900W','125mm 1400W','150mm 1700W','230mm 2200W','230mm 2600W'], NULL, 45,1.25,1.25,1,'piece',1,5,0.8),
('PTL','Circular Saw', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['165mm 1200W','190mm 1400W','235mm 2000W'], NULL, 95,1.25,1.4,1,'piece',1,3,0.4),
('PTL','Jigsaw', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['600W','750W','18V Bare Tool'], NULL, 59,1.25,1.3,1,'piece',1,3,0.4),
('PTL','Sander', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['125mm Random Orbit','150mm Random Orbit','1/3 Sheet Orbital','Belt 75mm'], NULL, 55,1.25,1.2,1,'piece',1,3,0.4),
('PTL','Demolition Hammer', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['5kg','10kg','16kg','30kg'], NULL, 189,1.25,1.5,1,'piece',1,2,0.3),
('PTL','Concrete Mixer', ARRAY['130L','160L','180L','220L','300L','350L'], ARRAY['230V','400V'], NULL, 249,1.2,1.25,1,'piece',1,2,0.3),
('PTL','Generator', ARRAY['2.2kW','3.5kW','5.5kW','8kW','12kW'], ARRAY['Petrol','Diesel'], NULL, 329,1.45,1.6,1,'piece',1,2,0.2),
('PTL','Pressure Washer', ARRAY['130bar','150bar','170bar','200bar'], ARRAY['Electric','Petrol'], NULL, 139,1.3,2.2,1,'piece',1,3,0.3),
('PTL','Battery Pack', ARRAY['12V','18V','36V'], ARRAY['2.0Ah','4.0Ah','5.0Ah','8.0Ah'], (SELECT v FROM seed_list WHERE k='brands'), 35,1.4,1.3,1.1,'piece',1,6,0.6),
('PTL','Battery Charger', (SELECT v FROM seed_list WHERE k='brands'), ARRAY['Standard','Fast','Dual-Port'], NULL, 29,1.1,1.5,1,'piece',1,4,0.3),
-- ---------------------------------------------------------------- HW: Power tool accessories
('ACC','HSS Drill Bit', ARRAY['1mm','1.5mm','2mm','2.5mm','3mm','3.5mm','4mm','4.5mm','5mm','5.5mm','6mm','6.5mm','7mm','7.5mm','8mm','8.5mm','9mm','9.5mm','10mm','10.5mm','11mm','11.5mm','12mm','12.5mm','13mm'], ARRAY['HSS-G','HSS-Co 5%','HSS-TiN'], NULL, 0.9,1.1,1.6,1,'pack of 2',1,30,1.5),
('ACC','SDS-plus Masonry Bit', ARRAY['5mm','6mm','6.5mm','8mm','10mm','12mm','14mm','16mm','18mm','20mm','22mm','25mm'], ARRAY['110mm','160mm','210mm','260mm','310mm','460mm'], NULL, 2.5,1.15,1.15,1,'piece',1,30,1.5),
('ACC','SDS-max Masonry Bit', ARRAY['12mm','14mm','16mm','18mm','20mm','22mm','25mm','28mm','32mm','35mm','40mm'], ARRAY['340mm','540mm','920mm'], NULL, 14,1.1,1.4,1,'piece',1,10,0.5),
('ACC','Wood Spade Bit', ARRAY['10mm','12mm','14mm','16mm','18mm','20mm','22mm','25mm','32mm','40mm'], NULL, NULL, 2.2,1.07,1,1,'piece',1,20,0.4),
('ACC','Bi-Metal Hole Saw', ARRAY['19mm','22mm','25mm','29mm','32mm','35mm','38mm','44mm','51mm','57mm','64mm','68mm','76mm','83mm','102mm','127mm'], NULL, NULL, 5.5,1.09,1,1,'piece',1,10,0.4),
('ACC','Cutting Disc', ARRAY['115mm','125mm','150mm','180mm','230mm','300mm','350mm'], ARRAY['Metal','Inox','Stone','Aluminium'], ARRAY['1.0mm','1.6mm','2.5mm','3.0mm'], 0.9,1.18,1.1,1.1,'piece',5,150,2.5),
('ACC','Grinding Disc', ARRAY['115mm','125mm','180mm','230mm'], ARRAY['Metal','Inox','Stone'], ARRAY['6mm'], 1.8,1.25,1.15,1,'piece',5,50,0.8),
('ACC','Flap Disc', ARRAY['115mm','125mm'], ARRAY['P40','P60','P80','P120'], ARRAY['Zirconia','Ceramic'], 2.0,1.1,1.0,1.5,'piece',5,50,0.8),
('ACC','Diamond Blade', ARRAY['115mm','125mm','150mm','180mm','230mm','300mm','350mm','400mm'], ARRAY['Universal Segmented','Turbo','Continuous Rim Tile','Asphalt','Concrete Laser-Welded'], NULL, 9.0,1.35,1.2,1,'piece',1,10,0.7),
('ACC','Circular Saw Blade', ARRAY['160mm','184mm','190mm','210mm','216mm','250mm','305mm'], ARRAY['24T','40T','48T','60T','80T'], ARRAY['Wood','Laminate','Aluminium'], 12,1.12,1.12,1.3,'piece',1,8,0.4),
('ACC','Jigsaw Blade', ARRAY['T101B','T101D','T111C','T118A','T127D','T144D','T244D','T301CD','T308B','T345XF'], ARRAY['Pack 5','Pack 25'], NULL, 3.5,1.05,4.2,1,'pack',1,20,0.6),
('ACC','Sanding Sheet', ARRAY['P40','P60','P80','P100','P120','P150','P180','P240','P320','P400'], ARRAY['125mm Disc 8-Hole','150mm Disc 15-Hole','93x230mm','115x280mm','225mm Drywall Disc'], ARRAY['Pack 10','Pack 50'], 3.0,1.0,1.15,4.2,'pack',1,30,1.2),
('ACC','Screwdriver Bit', ARRAY['PH1','PH2','PH3','PZ1','PZ2','PZ3','T10','T15','T20','T25','T30','T40','Hex 4','Hex 5','Hex 6'], ARRAY['25mm','50mm','90mm'], ARRAY['Pack 10'], 3.5,1.0,1.5,1,'pack',1,25,1.0),
-- ---------------------------------------------------------------- HW: Paints & coatings
('PNT','Interior Wall Paint', (SELECT v FROM seed_list WHERE k='colors'), ARRAY['1L','2.5L','5L','10L','15L'], ARRAY['Matte','Silk'], 7.0,1.0,1.9,1.15,'bucket',1,40,2.0),
('PNT','Facade Paint', (SELECT v[1:10] FROM seed_list WHERE k='colors'), ARRAY['5L','15L'], ARRAY['Acrylic','Silicone'], 28,1.0,2.6,1.5,'bucket',1,30,1.0),
('PNT','Wood Stain', ARRAY['Natural','Pine','Oak','Teak','Walnut','Mahogany','Rosewood','Ebony'], ARRAY['0.75L','2.5L','5L'], NULL, 9.0,1.0,2.7,1,'can',1,20,0.7),
('PNT','Metal Enamel', (SELECT v FROM seed_list WHERE k='colors'), ARRAY['0.75L','2.5L'], ARRAY['Gloss','Satin'], 8.5,1.0,2.8,1.05,'can',1,20,0.8),
('PNT','Primer', ARRAY['Deep Penetrating','Acrylic Universal','Anti-Rust','Wood Primer','Concrete Contact'], ARRAY['1L','5L','10L','20L'], NULL, 6.0,1.15,1.8,1,'bucket',1,25,1.0),
('PNT','Spray Paint 400ml', (SELECT v || ARRAY['Silver','Gold','Clear Lacquer','Fluorescent Orange'] FROM seed_list WHERE k='colors'), ARRAY['Gloss','Matte'], NULL, 4.2,1.0,1.0,1,'can',1,30,1.0),
('PNT','Paint Brush', ARRAY['20mm','30mm','40mm','50mm','60mm','70mm','80mm','100mm'], ARRAY['Synthetic','Natural Bristle','Angled Sash'], NULL, 1.5,1.18,1.2,1,'piece',1,40,1.0),
('PNT','Paint Roller', ARRAY['10cm','18cm','25cm'], ARRAY['Short Pile','Medium Pile','Long Pile','Foam'], NULL, 2.5,1.4,1.1,1,'piece',1,30,0.9),
('PNT','Thinner', ARRAY['Nitro','Synthetic','White Spirit','Acetone','Isopropyl Alcohol'], ARRAY['1L','5L','20L'], NULL, 3.0,1.05,4.0,1,'can',1,20,0.7),
('PNT','Masking Tape', ARRAY['19mm','25mm','30mm','38mm','48mm'], ARRAY['50m Standard','50m UV Resistant','50m Washi'], NULL, 1.4,1.15,1.6,1,'roll',2,60,1.2),
-- ---------------------------------------------------------------- HW: Plumbing
('PLB','PPR Pipe', ARRAY['20mm','25mm','32mm','40mm','50mm','63mm','75mm','90mm'], ARRAY['PN10','PN20','PN20 Fibre'], ARRAY['4m'], 2.2,1.45,1.3,1,'piece',5,200,1.5),
('PLB','PPR Fitting', ARRAY['Elbow 90°','Elbow 45°','Tee','Coupling','Reducer','End Cap','Male Adapter','Female Adapter','Union','Crossover'], ARRAY['20mm','25mm','32mm','40mm','50mm','63mm'], NULL, 0.35,1.08,1.5,1,'piece',10,300,1.5),
('PLB','PVC Drain Pipe', ARRAY['32mm','40mm','50mm','75mm','110mm','125mm','160mm','200mm'], ARRAY['0.5m','1m','2m','3m'], NULL, 1.2,1.35,1.7,1,'piece',2,100,1.2),
('PLB','PVC Drain Fitting', ARRAY['Bend 45°','Bend 87°','Branch 45°','Reducer','Cleaning Eye','Socket Plug'], ARRAY['32mm','40mm','50mm','75mm','110mm','125mm','160mm'], NULL, 0.6,1.1,1.35,1,'piece',5,150,1.2),
('PLB','Ball Valve', ARRAY['1/2"','3/4"','1"','1 1/4"','1 1/2"','2"'], ARRAY['Brass','Chrome Plated'], ARRAY['Lever Handle','Butterfly Handle'], 4.5,1.45,1.15,0.95,'piece',1,30,1.0),
('PLB','Flexible Hose', ARRAY['30cm','40cm','50cm','60cm','80cm','100cm'], ARRAY['1/2" F-F','3/8" F-F','1/2" M-F'], NULL, 2.2,1.1,1.0,1,'piece',2,50,1.0),
('PLB','Copper Pipe', ARRAY['12mm','15mm','18mm','22mm','28mm','35mm','42mm'], ARRAY['Straight 5m','Coil 25m'], NULL, 22,1.35,4.5,1,'piece',1,20,0.5),
('PLB','PEX-AL-PEX Pipe', ARRAY['16x2mm','20x2mm','26x3mm','32x3mm'], ARRAY['Coil 100m','Coil 200m','Straight 5m'], NULL, 85,1.45,1.0,1,'piece',1,10,0.6),
('PLB','Press Fitting', ARRAY['Elbow','Tee','Coupling','Male Adapter','Female Adapter','Wall Plate'], ARRAY['16mm','20mm','26mm','32mm'], NULL, 2.8,1.08,1.35,1,'piece',5,100,0.8),
('PLB','Basin Mixer Tap', ARRAY['Standard','High Spout','Wall Mounted','Sensor'], ARRAY['Chrome','Matte Black','Brushed Steel'], NULL, 39,1.35,1.2,1,'piece',1,8,0.4),
('PLB','Shower Mixer', ARRAY['Exposed','Concealed','Thermostatic'], ARRAY['Chrome','Matte Black'], NULL, 49,1.4,1.2,1,'piece',1,6,0.3),
('PLB','PTFE Tape', ARRAY['12mm x 12m','19mm x 15m'], NULL, NULL, 0.6,1.5,1,1,'roll',5,100,1.0),
('PLB','Silicone Sealant 280ml', ARRAY['Sanitary White','Sanitary Transparent','Neutral Grey','Construction Black','High-Temp Red','Acrylic White'], NULL, NULL, 4.2,1.04,1,1,'cartridge',2,60,1.5),
-- ---------------------------------------------------------------- HW: Electrical
('ELC','Installation Cable NYM-J', ARRAY['3x1.5mm²','3x2.5mm²','4x1.5mm²','5x1.5mm²','5x2.5mm²','5x4mm²','5x6mm²','5x10mm²','5x16mm²'], ARRAY['Coil 50m','Coil 100m','Drum 500m'], NULL, 32,1.35,1.9,1,'coil',1,20,1.5),
('ELC','Flexible Cable H07RN-F', ARRAY['3G1.5','3G2.5','5G1.5','5G2.5','5G4','5G6'], ARRAY['Coil 50m','Drum 250m'], NULL, 55,1.3,4.6,1,'coil',1,10,0.6),
('ELC','Power Cable NYY-J', ARRAY['3x1.5','3x2.5','5x2.5','5x6','5x10','4x25','4x50','4x95'], ARRAY['Coil 100m','Drum 500m'], NULL, 75,1.55,4.7,1,'coil',1,6,0.4),
('ELC','Circuit Breaker', ARRAY['B6A','B10A','B13A','B16A','B20A','B25A','B32A','B40A','C10A','C16A','C20A','C25A','C32A','C40A','C50A','C63A'], ARRAY['1P','1P+N','3P'], NULL, 3.2,1.03,1.8,1,'piece',2,60,1.5),
('ELC','Residual Current Device', ARRAY['25A','40A','63A'], ARRAY['30mA','100mA','300mA'], ARRAY['2P','4P'], 22,1.2,1.0,1.7,'piece',1,15,0.6),
('ELC','Wall Socket', ARRAY['Single Schuko','Double Schuko','Single Schuko IP44','USB-A/C Combo','TV/SAT','RJ45 Cat6'], ARRAY['White','Ivory','Anthracite','Aluminium'], NULL, 3.5,1.25,1.15,1,'piece',2,60,1.2),
('ELC','Light Switch', ARRAY['One-Way','Two-Way','Double One-Way','Cross','Push Button','Dimmer'], ARRAY['White','Ivory','Anthracite','Aluminium'], NULL, 2.8,1.2,1.15,1,'piece',2,60,1.2),
('ELC','LED Bulb', ARRAY['E27 9W','E27 12W','E27 15W','E14 5W','E14 7W','GU10 5W','GU10 7W','G9 3W'], ARRAY['2700K','4000K','6500K'], NULL, 1.9,1.05,1.0,1,'piece',5,120,2.0),
('ELC','LED Panel', ARRAY['600x600 36W','600x600 40W','1200x300 40W','Round 18W','Round 24W'], ARRAY['4000K','6500K'], ARRAY['Recessed','Surface'], 19,1.0,1.0,1.2,'piece',1,40,0.8),
('ELC','LED Floodlight', ARRAY['10W','20W','30W','50W','100W','150W','200W'], ARRAY['IP65','IP65 with Sensor'], NULL, 7.0,1.4,1.4,1,'piece',1,20,0.7),
('ELC','Cable Tie', ARRAY['2.5x100','3.6x150','3.6x200','4.8x200','4.8x300','7.6x370','9x780'], ARRAY['Black UV','Natural'], ARRAY['Pack 100'], 0.9,1.35,1.1,1,'pack',2,80,1.5),
('ELC','Electrical Conduit', ARRAY['16mm','20mm','25mm','32mm','40mm','50mm'], ARRAY['Flexible Corrugated 50m','Rigid 3m'], NULL, 9.0,1.3,0.12,1,'piece',2,60,1.0),
('ELC','Junction Box', ARRAY['80x80','100x100','150x110','190x140','240x190'], ARRAY['IP44','IP65'], NULL, 1.6,1.4,1.5,1,'piece',2,50,0.8),
('ELC','Extension Cord', ARRAY['3m','5m','10m','25m','50m'], ARRAY['3-Way','5-Way','Cable Reel'], NULL, 6.0,1.35,1.3,1,'piece',1,15,0.7),
('ELC','Insulating Tape', ARRAY['Black','Red','Blue','Yellow-Green','White'], ARRAY['19mm x 10m','19mm x 20m'], NULL, 0.7,1.0,1.7,1,'roll',5,100,1.2),
-- ---------------------------------------------------------------- HW: PPE
('PPE','Work Gloves', ARRAY['Nitrile Coated','Latex Coated','Leather','Cut Resistant Level C','Winter Insulated','PU Coated Precision'], ARRAY['Size 7','Size 8','Size 9','Size 10','Size 11'], ARRAY['Pair','Pack 12'], 2.0,1.25,1.0,10,'pack',2,60,2.0),
('PPE','Safety Boots', ARRAY['S1P Low','S3 Mid','S3 High Winter'], ARRAY['Size 38','Size 39','Size 40','Size 41','Size 42','Size 43','Size 44','Size 45','Size 46','Size 47'], NULL, 39,1.25,1.0,1,'pair',1,15,1.0),
('PPE','Safety Helmet', ARRAY['White','Yellow','Red','Blue','Orange','Green'], ARRAY['Standard','Vented','With Chin Strap'], NULL, 6.5,1.0,1.25,1,'piece',1,40,1.0),
('PPE','High-Visibility Vest', ARRAY['Yellow','Orange'], ARRAY['M','L','XL','XXL','3XL'], NULL, 3.0,1.0,1.03,1,'piece',2,60,1.2),
('PPE','Safety Goggles', ARRAY['Clear','Tinted','Over-Spectacle','Sealed Chemical'], NULL, NULL, 3.5,1.3,1,1,'piece',2,50,0.8),
('PPE','Hearing Protection', ARRAY['Earmuffs SNR 27','Earmuffs SNR 33','Earplugs Box 200','Helmet-Mounted Earmuffs'], NULL, NULL, 9.0,1.3,1,1,'piece',1,30,0.6),
('PPE','Respirator Mask', ARRAY['FFP1','FFP2','FFP3'], ARRAY['Without Valve','With Valve'], ARRAY['Box 10','Box 20'], 5.0,1.5,1.4,1.9,'box',1,30,1.2),
('PPE','Work Trousers', ARRAY['Grey/Black','Navy','Green','Hi-Vis Orange'], ARRAY['46','48','50','52','54','56','58','60'], NULL, 29,1.05,1.0,1,'piece',1,20,0.7),
('PPE','Fall Arrest Harness', ARRAY['1-Point','2-Point','Full Body 5-Point'], NULL, NULL, 45,1.4,1,1,'piece',1,6,0.3),
-- ---------------------------------------------------------------- BLK: Cement & mortars
('CEM','Cement', ARRAY['CEM I 42.5R','CEM II/A-M 42.5R','CEM II/B-M 32.5R','CEM III/A 42.5N','White Cement 52.5R'], ARRAY['25kg Bag','40kg Bag','50kg Bag'], NULL, 4.2,1.08,1,1,'bag',10,400,3.0),
('CEM','Cement Big Bag 1t', ARRAY['CEM I 42.5R','CEM II/A-M 42.5R','CEM II/B-M 32.5R','CEM III/A 42.5N','White Cement 52.5R'], NULL, NULL, 135,1.08,1,1,'big bag',1,24,1.0),
('CEM','Bulk Cement per Tonne', ARRAY['CEM I 42.5R','CEM II/A-M 42.5R','CEM II/B-M 32.5R'], NULL, NULL, 118,1.05,1,1,'tonne',5,30,1.0),
('CEM','Masonry Mortar', ARRAY['M5','M10','M15','Thin-Bed AAC'], ARRAY['25kg Bag','40kg Bag'], NULL, 3.8,1.15,1.5,1,'bag',10,200,1.5),
('CEM','Tile Adhesive', ARRAY['C1T','C2T','C2TE','C2TE S1','C2TE S2','Flexible Rapid'], ARRAY['Grey','White'], ARRAY['5kg','25kg'], 4.0,1.2,1.3,3.6,'bag',5,150,2.0),
('CEM','Tile Grout', ARRAY['White','Jasmine','Bahama Beige','Grey','Silver Grey','Cement Grey','Anthracite','Black','Caramel','Brown','Terracotta','Light Blue'], ARRAY['2kg','5kg','20kg'], NULL, 3.5,1.0,2.2,1,'bag',2,60,1.0),
('CEM','Gypsum Plaster', ARRAY['Machine Plaster','Hand Plaster','Finishing Plaster','Quick-Set'], ARRAY['25kg','30kg'], NULL, 6.5,1.1,1.18,1,'bag',10,200,1.2),
('CEM','Cement Render', ARRAY['Base Coat','Lime-Cement','Decorative 1.5mm','Decorative 2.0mm','Silicate 1.5mm'], ARRAY['25kg Bag'], NULL, 5.0,1.35,1,1,'bag',10,200,1.0),
('CEM','Self-Leveling Screed', ARRAY['1-10mm','3-30mm','5-50mm','Fibre Reinforced'], ARRAY['25kg Bag'], NULL, 11,1.12,1,1,'bag',5,120,0.8),
('CEM','Waterproofing Slurry', ARRAY['Rigid','Flexible 1K','Flexible 2K'], ARRAY['5kg','20kg','25kg'], NULL, 12,1.5,3.2,1.2,'bag',1,40,0.6),
-- ---------------------------------------------------------------- BLK: Aggregates
('AGG','Sand 25kg Bag', ARRAY['0-2mm Washed','0-4mm Washed','0-4mm River','Quartz 0.1-0.3mm'], NULL, NULL, 2.4,1.1,1,1,'bag',10,200,0.8),
('AGG','Sand Bulk per Tonne', ARRAY['0-2mm Washed','0-4mm Washed','0-4mm River','Quartz 0.1-0.3mm'], NULL, NULL, 14,1.15,1,1,'tonne',5,60,1.2),
('AGG','Gravel Bulk per Tonne', ARRAY['4-8mm','8-16mm','16-32mm','0-31.5mm Crushed','0-63mm Crushed','Decorative White 16-32mm'], NULL, NULL, 13,1.12,1,1,'tonne',5,60,1.2),
('AGG','Gravel 1t Big Bag', ARRAY['4-8mm','8-16mm','16-32mm','0-31.5mm Crushed','0-63mm Crushed','Decorative White 16-32mm'], NULL, NULL, 45,1.08,1,1,'big bag',1,20,0.6),
-- ---------------------------------------------------------------- BLK: Bricks, blocks & paving
('BRK','Clay Hollow Block', ARRAY['190mm','250mm','290mm','380mm'], ARRAY['Standard','Thermal','Acoustic'], NULL, 0.75,1.22,1.25,1,'piece',100,3000,2.0),
('BRK','Aerated Concrete Block', ARRAY['75mm','100mm','125mm','150mm','200mm','250mm','300mm','375mm'], ARRAY['Density 400','Density 500'], NULL, 1.6,1.25,1.08,1,'piece',50,1500,1.8),
('BRK','Concrete Block', ARRAY['Hollow 190mm','Hollow 250mm','Solid 100mm','Lintel U-Block'], NULL, NULL, 1.1,1.25,1,1,'piece',50,1500,1.0),
('BRK','Facing Brick', ARRAY['Red Smooth','Red Rustic','Yellow','Brown','Anthracite','Antique Blend'], ARRAY['NF 240x115x71','DF 240x115x52'], NULL, 0.65,1.06,0.9,1,'piece',100,4000,0.8),
('BRK','Paving Stone', ARRAY['Holland 6cm','Holland 8cm','Behaton 6cm','Behaton 8cm','Classic Cobble 6cm','Large Slab 4cm'], ARRAY['Grey','Red','Anthracite','Ochre','Mix'], ARRAY['per m²'], 9.5,1.1,1.04,1,'m²',10,400,1.5),
('BRK','Kerbstone', ARRAY['100x25x15','100x20x8','50x20x6'], ARRAY['Grey','Red'], NULL, 6.0,0.7,1.1,1,'piece',10,300,0.6),
-- ---------------------------------------------------------------- BLK: Insulation
('INS','EPS Insulation Board', ARRAY['2cm','3cm','4cm','5cm','8cm','10cm','12cm','14cm','15cm','16cm','18cm','20cm'], ARRAY['EPS 70','EPS 80','EPS 100','EPS 150','Graphite EPS 80'], ARRAY['per m²'], 0.8,1.25,1.15,1,'m²',10,800,1.8),
('INS','XPS Insulation Board', ARRAY['2cm','3cm','4cm','5cm','6cm','8cm','10cm','12cm','14cm','16cm'], ARRAY['Flat Edge','Stepped Edge'], ARRAY['per m²'], 2.4,1.2,1.12,1,'m²',10,500,1.0),
('INS','Mineral Wool', ARRAY['5cm','8cm','10cm','12cm','14cm','15cm','16cm','18cm','20cm'], ARRAY['Roll λ0.040','Roll λ0.035','Slab λ0.037','Facade Slab λ0.036'], ARRAY['per m²'], 2.2,1.17,1.25,1,'m²',10,600,1.5),
('INS','Facade Mesh', ARRAY['145g/m²','160g/m²','165g/m²'], ARRAY['50m² Roll'], NULL, 32,1.08,1,1,'roll',1,60,0.8),
('INS','Facade Anchor', ARRAY['8x90','8x110','8x130','8x150','8x170','8x190','8x210','8x230','8x250'], ARRAY['Plastic Nail','Steel Nail'], ARRAY['Box 100','Box 250'], 9.0,1.07,1.35,2.3,'box',1,40,0.8),
('INS','Roof Membrane', ARRAY['Vapour Barrier PE 0.2mm','Breather 115g','Breather 150g','Breather 180g','Vapour Control 110g'], ARRAY['75m² Roll','50m² Roll'], NULL, 35,1.15,0.72,1,'roll',1,40,0.7),
-- ---------------------------------------------------------------- BLK: Drywall systems
('DRY','Gypsum Board', ARRAY['Standard','Moisture Resistant','Fire Resistant','Fire+Moisture','Impact','Acoustic'], ARRAY['9.5mm','12.5mm','15mm','18mm'], ARRAY['2000x1200','2600x1200','3000x1200'], 5.2,1.15,1.15,1.25,'sheet',5,300,2.0),
('DRY','Drywall Profile', ARRAY['CW 50','CW 75','CW 100','UW 50','UW 75','UW 100','CD 60/27','UD 28/27','UA 50','UA 75'], ARRAY['0.6mm'], ARRAY['2.6m','3m','4m'], 2.2,1.06,1,1.2,'piece',10,500,1.8),
('DRY','Joint Compound', ARRAY['Standard','Lightweight','Fine Finish','Fibre Tape-Free'], ARRAY['5kg','20kg','25kg'], NULL, 6.0,1.15,3.0,1.15,'bag',2,100,1.0),
('DRY','Drywall Tape', ARRAY['Paper 50mm x 75m','Fibreglass 50mm x 90m','Corner Bead 2.5m','Flexible Corner 30m'], NULL, NULL, 3.2,1.1,1,1,'piece',2,100,0.8),
-- ---------------------------------------------------------------- BLK: Timber & boards
('TMB','OSB/3 Board', ARRAY['8mm','9mm','10mm','12mm','15mm','18mm','22mm','25mm'], ARRAY['2500x1250 Straight','2500x1250 T&G','2440x1220 Straight'], NULL, 9.0,1.15,1.05,1,'sheet',5,400,2.0),
('TMB','Plywood', ARRAY['Birch','Poplar','Pine','Film-Faced Brown','Film-Faced Anti-Slip'], ARRAY['4mm','6mm','9mm','12mm','15mm','18mm','21mm','24mm','30mm'], ARRAY['2500x1250'], 14,1.1,1.25,1,'sheet',2,200,1.2),
('TMB','MDF Board', ARRAY['3mm','6mm','8mm','10mm','12mm','16mm','18mm','22mm','25mm'], ARRAY['Raw','White Primed','Moisture Resistant'], ARRAY['2800x2070'], 8.0,1.2,1.3,1,'sheet',2,150,0.8),
('TMB','Melamine Chipboard', ARRAY['White','Black','Light Grey','Anthracite','Sonoma Oak','Wenge','Beech','Walnut','Cherry','Concrete Look','Cashmere','Natural Oak'], ARRAY['10mm','16mm','18mm','25mm'], ARRAY['2800x2070'], 22,1.02,1.15,1,'sheet',2,150,1.0),
('TMB','Construction Timber KVH', ARRAY['60x80','60x120','60x140','60x160','80x80','80x120','80x160','80x200','100x100','100x120','120x120','120x160','120x200','140x140','160x160','160x200'], ARRAY['3m','4m','5m','6m','8m','10m','13m'], NULL, 7.0,1.12,1.3,1,'piece',2,150,1.5),
('TMB','Roof Batten', ARRAY['24x48','30x50','40x60','50x80'], ARRAY['4m','5m'], ARRAY['Spruce','Impregnated'], 1.6,1.45,1.25,1.3,'piece',20,1000,1.0),
('TMB','Glulam Beam GL24h', ARRAY['100x200','120x240','140x280','160x320','200x400'], ARRAY['6m','8m','10m','12m'], NULL, 110,1.45,1.33,1,'piece',1,20,0.4),
('TMB','3-Layer Formwork Panel', ARRAY['21mm 2000x500','21mm 2500x500','27mm 2000x500','27mm 2500x500','21mm 1500x500'], NULL, NULL, 19,1.1,1,1,'piece',10,300,0.8),
('TMB','Decking Board', ARRAY['Spruce Impregnated 27x145','Larch 27x140','Thermowood 26x140','WPC Composite 25x140'], ARRAY['3m','4m','5m'], ARRAY['Smooth','Grooved'], 7.0,1.4,1.3,1.05,'piece',5,200,0.6),
-- ---------------------------------------------------------------- BLK: Roofing & drainage
('ROF','Concrete Roof Tile', ARRAY['Red','Brick Red','Brown','Anthracite','Black'], ARRAY['Standard','Ridge','Verge Left','Verge Right','Ventilation'], NULL, 0.95,1.0,1.9,1,'piece',50,3000,1.5),
('ROF','Clay Roof Tile', ARRAY['Natural Red','Engobe Red','Engobe Brown','Engobe Black','Glazed Anthracite'], ARRAY['Interlocking','Ridge','Half Tile','Ventilation'], NULL, 1.4,1.12,1.8,1,'piece',50,3000,1.2),
('ROF','Trapezoidal Sheet', ARRAY['T18','T20','T35','T45'], ARRAY['0.4mm','0.5mm','0.6mm'], (SELECT array_replace(v, 'Unpainted', 'Galvanized') FROM seed_list WHERE k='ral'), 6.5,1.12,1.2,1.0,'m²',20,1500,1.2),
('ROF','Bitumen Membrane', ARRAY['V13 Glass Fleece','G200 S4','P4 Polyester','SBS 4mm Mineral','APP 4mm','Self-Adhesive 3mm'], ARRAY['10m² Roll','7.5m² Roll'], NULL, 28,1.15,0.8,1,'roll',1,80,1.0),
('ROF','Gutter System', ARRAY['Gutter 3m','Downpipe 3m','Gutter Bracket','Gutter End Cap','Gutter Outlet','Downpipe Elbow 70°','Downpipe Clip'], ARRAY['Ø100','Ø125','Ø150'], ARRAY['PVC White','PVC Brown','Steel RAL 8017','Steel RAL 7016','Galvanized'], 9.0,0.62,1.2,1.15,'piece',2,100,1.0),
-- ---------------------------------------------------------------- BLK: Reinforcement & steel
('STL','Rebar B500B', ARRAY['Ø6','Ø8','Ø10','Ø12','Ø14','Ø16','Ø20','Ø25','Ø28','Ø32'], ARRAY['6m','12m'], NULL, 2.2,1.35,2.0,1,'bar',10,2000,2.0),
('STL','Reinforcing Mesh', ARRAY['Q131','Q188','Q257','Q335','Q385','R131','R188'], ARRAY['6x2.15m','5x2.15m'], NULL, 32,1.25,0.85,1,'sheet',5,200,1.2),
('STL','Binding Wire', ARRAY['1.2mm','1.4mm','1.6mm','2.0mm'], ARRAY['Coil 2kg','Coil 25kg'], NULL, 4.5,1.05,10,1,'coil',1,60,0.8),
('STL','Steel Profile', ARRAY['Square 20x20','Square 30x30','Square 40x40','Square 50x50','Square 60x60','Rect 40x20','Rect 60x40','Rect 80x40','Rect 100x50','Rect 120x60','Round 21.3','Round 33.7','Round 48.3','Angle 30x30','Angle 40x40','Angle 50x50','Flat 30x4','Flat 40x5','IPE 100','IPE 140','IPE 200','UPN 100','UPN 140','HEA 140'], ARRAY['6m','12m'], NULL, 14,1.12,2.0,1,'piece',1,100,1.0),
-- ---------------------------------------------------------------- BLK: Bulk liquids & containers
('LIQ','AdBlue', ARRAY['10L Canister','20L Canister','210L Drum','1000L IBC'], NULL, NULL, 9.0,1,1,1,'unit',1,40,1.5),
('LIQ','Hydraulic Oil HLP', ARRAY['ISO VG 22','ISO VG 32','ISO VG 46','ISO VG 68','ISO VG 100'], ARRAY['20L Canister','208L Drum','1000L IBC'], NULL, 48,1.03,1,1,'unit',1,10,0.8),
('LIQ','Formwork Release Oil', ARRAY['Mineral','Bio-Degradable'], ARRAY['20L Canister','208L Drum','1000L IBC'], NULL, 38,1.4,1,1,'unit',1,10,0.6),
('LIQ','Concrete Admixture', ARRAY['Plasticizer','Superplasticizer','Accelerator','Retarder','Air Entrainer','Waterproofing','Antifreeze'], ARRAY['25kg Canister','200kg Drum','1000kg IBC'], NULL, 32,1.08,1,1,'unit',1,10,0.6),
('LIQ','Bitumen Emulsion', ARRAY['Primer','Cationic 60%','Rubberized'], ARRAY['5L','20L','200L Drum','1000L IBC'], NULL, 12,1.25,1,1,'unit',1,15,0.6),
('LIQ','Liquid Waterproofing', ARRAY['PU Membrane','Acrylic Roof Coating','Hybrid Polymer'], ARRAY['5kg','25kg'], ARRAY['Grey','White','Red'], 45,0.7,4.2,1.0,'pail',1,20,0.5),
('LIQ','Industrial Degreaser', ARRAY['Alkaline','Solvent-Based','Citrus Bio'], ARRAY['5L','25L','200L Drum','1000L IBC'], NULL, 14,1.2,1,1,'unit',1,15,0.6),
('LIQ','IBC Container', ARRAY['1000L New','1000L Reconditioned','600L New'], NULL, NULL, 210,0.6,1,1,'piece',1,20,0.4),
('LIQ','Plastic Drum', ARRAY['60L','120L','220L'], ARRAY['Open Top','Closed Top'], NULL, 18,1.5,0.9,1,'piece',1,40,0.4),
('LIQ','Jerrycan', ARRAY['10L','20L','25L','30L'], ARRAY['Natural','Blue','Black UN-Approved'], NULL, 4.5,1.3,1.1,1,'piece',2,100,0.6),
-- ---------------------------------------------------------------- AUT
('FLT','Oil Filter',  (SELECT v FROM seed_list WHERE k='cars'), ARRAY['Economy','OE Quality'], NULL, 6.0,1.01,1.6,1,'piece',1,30,2.0),
('FLT','Air Filter',  (SELECT v FROM seed_list WHERE k='cars'), ARRAY['Economy','OE Quality'], NULL, 8.5,1.01,1.6,1,'piece',1,25,1.5),
('FLT','Cabin Filter',(SELECT v FROM seed_list WHERE k='cars'), ARRAY['Standard','Activated Carbon'], NULL, 7.5,1.01,1.7,1,'piece',1,25,1.5),
('FLT','Fuel Filter', (SELECT v FROM seed_list WHERE k='cars'), ARRAY['OE Quality'], NULL, 14,1.01,1,1,'piece',1,15,1.0),
('BRA','Brake Pad Set', (SELECT v FROM seed_list WHERE k='cars'), ARRAY['Front','Rear'], ARRAY['Economy','OE Quality','Premium Ceramic'], 22,1.015,0.8,1.4,'set',1,20,1.5),
('BRA','Brake Disc',    (SELECT v FROM seed_list WHERE k='cars'), ARRAY['Front','Rear'], ARRAY['Standard','Coated'], 32,1.015,0.8,1.25,'piece',1,12,1.0),
('SUS','Shock Absorber',(SELECT v FROM seed_list WHERE k='cars'), ARRAY['Front','Rear'], ARRAY['Gas','Gas Sport'], 45,1.015,0.75,1.3,'piece',1,8,0.7),
('ENG','Timing Belt Kit',(SELECT v FROM seed_list WHERE k='cars'), ARRAY['Without Water Pump','With Water Pump'], NULL, 65,1.015,1.6,1,'kit',1,6,0.6),
('ENG','Clutch Kit',    (SELECT v FROM seed_list WHERE k='cars'), ARRAY['3-Piece','With Dual-Mass Flywheel'], NULL, 140,1.02,3.0,1,'kit',1,4,0.4),
('ENG','Spark Plug', ARRAY['Nickel','Platinum','Iridium'], ARRAY['14mm Thread 16mm Hex','14mm Thread 14mm Hex','12mm Thread 14mm Hex','10mm Thread 16mm Hex'], NULL, 3.2,2.0,1.05,1,'piece',4,60,1.2),
('ENG','Glow Plug', ARRAY['M8','M10','M12'], ARRAY['Standard','Ceramic'], NULL, 9.0,1.1,2.2,1,'piece',4,40,0.6),
('LGT','Wiper Blade', ARRAY['350mm','380mm','400mm','425mm','450mm','475mm','500mm','530mm','550mm','600mm','650mm','700mm'], ARRAY['Conventional','Flat','Hybrid'], NULL, 5.5,1.03,1.5,1,'piece',2,40,1.5),
('LGT','Headlight Bulb', ARRAY['H1','H3','H4','H7','H11','HB3','HB4','D1S','D2S','D3S'], ARRAY['Standard','+100% Brightness','White Vision'], NULL, 3.5,1.15,1.6,1,'piece',2,50,1.5),
('LGT','Signal Bulb', ARRAY['P21W','P21/5W','PY21W','W5W','W21W','R5W','C5W 36mm'], ARRAY['Box 10'], NULL, 3.0,1.03,1,1,'box',1,30,1.0),
('BAT','Car Battery', ARRAY['44Ah 440A','50Ah 450A','60Ah 540A','70Ah 640A','74Ah 680A','80Ah 740A','95Ah 800A','100Ah 830A'], ARRAY['Standard','EFB Start-Stop','AGM Start-Stop'], NULL, 62,1.1,1.35,1,'piece',1,15,1.2),
('BAT','Truck Battery', ARRAY['140Ah 800A','180Ah 1000A','225Ah 1150A'], ARRAY['Heavy Duty','EFB'], NULL, 189,1.2,1.25,1,'piece',1,6,0.5),
('OIL','Engine Oil', ARRAY['0W-20','0W-30','5W-30','5W-30 C3','5W-40','10W-40','15W-40','10W-30 Truck'], ARRAY['1L','4L','5L'], NULL, 9.0,1.0,1,1,'bottle',1,60,2.5),
('OIL','Engine Oil Bulk', ARRAY['0W-20','0W-30','5W-30','5W-30 C3','5W-40','10W-40','15W-40','10W-30 Truck'], ARRAY['20L Canister','60L Drum','208L Drum'], NULL, 120,0.97,1,1,'unit',1,12,1.0),
('OIL','Gear Oil', ARRAY['75W-80','75W-90','80W-90','85W-140','ATF Dexron III','ATF Dexron VI'], ARRAY['1L','20L Canister','208L Drum'], NULL, 11,1.03,1,1,'unit',1,24,0.8),
('OIL','Coolant Antifreeze', ARRAY['G11 Blue','G12 Red','G12+ Pink','G13 Violet'], ARRAY['Concentrate 1L','Concentrate 5L','Ready-Mix 5L','Concentrate 208L Drum'], NULL, 5.0,1.1,1,1,'unit',1,40,1.0),
('OIL','Brake Fluid', ARRAY['DOT 3','DOT 4','DOT 4 LV','DOT 5.1'], ARRAY['0.5L','1L','5L'], NULL, 4.0,1.15,1,1,'bottle',1,40,0.8),
('OIL','Windshield Washer Fluid', ARRAY['Summer','Winter -20°C','Winter -40°C Concentrate'], ARRAY['5L','25L','1000L IBC'], NULL, 3.5,1.4,1,1,'unit',1,60,1.2),
('TYR','Tyre', ARRAY['155/70 R13','165/70 R14','175/65 R14','185/65 R15','195/65 R15','205/55 R16','215/55 R17','225/45 R17','225/50 R17','235/55 R18','245/40 R18','255/55 R18','195/75 R16C','215/65 R16C','225/65 R16C'], ARRAY['Summer','Winter','All-Season'], ARRAY['Economy','Premium'], 45,1.08,1.05,1.6,'piece',1,40,1.5),
('TYR','Truck Tyre', ARRAY['315/70 R22.5','315/80 R22.5','385/65 R22.5','295/80 R22.5','265/70 R19.5'], ARRAY['Steer','Drive','Trailer'], NULL, 320,1.04,0.95,1,'piece',1,12,0.5),
('TRK','Truck Oil Filter',      (SELECT v FROM seed_list WHERE k='trucks'), NULL, NULL, 24,1.03,1,1,'piece',1,20,0.8),
('TRK','Truck Air Filter',      (SELECT v FROM seed_list WHERE k='trucks'), NULL, NULL, 58,1.03,1,1,'piece',1,10,0.6),
('TRK','Truck Fuel Filter',     (SELECT v FROM seed_list WHERE k='trucks'), NULL, NULL, 32,1.03,1,1,'piece',1,15,0.6),
('TRK','Truck Brake Pad Set',   (SELECT v FROM seed_list WHERE k='trucks'), ARRAY['Front','Rear'], NULL, 95,1.03,1.0,1,'set',1,8,0.5),
('TRK','Truck Brake Disc',      (SELECT v FROM seed_list WHERE k='trucks'), ARRAY['Front','Rear'], NULL, 145,1.03,1.0,1,'piece',1,6,0.4),
('TRK','Air Dryer Cartridge',   (SELECT v FROM seed_list WHERE k='trucks'), NULL, NULL, 38,1.02,1,1,'piece',1,10,0.4),
-- ---------------------------------------------------------------- IND
('BRG','Deep Groove Ball Bearing', ARRAY['6000','6001','6002','6003','6004','6005','6006','6007','6008','6010','6200','6201','6202','6203','6204','6205','6206','6207','6208','6210','6300','6301','6302','6303','6304','6305','6306','6308','6310'], ARRAY['Open','2RS','ZZ'], ARRAY['Economy','Premium'], 1.8,1.07,1.15,2.2,'piece',2,100,1.5),
('BRG','Tapered Roller Bearing', ARRAY['30202','30203','30204','30205','30206','30207','30208','32005','32006','32007','32008','32206','32207','32208'], NULL, NULL, 6.5,1.08,1,1,'piece',1,40,0.6),
('BRG','Pillow Block Bearing', ARRAY['UCP204','UCP205','UCP206','UCP207','UCP208','UCP210','UCF204','UCF205','UCF206','UCF208'], NULL, NULL, 9.0,1.1,1,1,'piece',1,20,0.5),
('PTR','V-Belt', ARRAY['SPZ','SPA','SPB','A/13','B/17'], ARRAY['1000','1250','1400','1600','1800','2000','2240','2500','2800','3150'], NULL, 6.0,1.25,1.06,1,'piece',1,30,1.0),
('HYD','Hydraulic Hose', ARRAY['DN6 1/4"','DN8 5/16"','DN10 3/8"','DN12 1/2"','DN16 5/8"','DN20 3/4"','DN25 1"'], ARRAY['1SN','2SN','4SP'], ARRAY['per Meter','Roll 50m'], 3.2,1.25,1.4,42,'unit',1,60,1.0),
('HYD','Hydraulic Fitting', ARRAY['Straight','90° Elbow','45° Elbow','Tee'], ARRAY['DN6','DN8','DN10','DN12','DN16','DN20','DN25'], ARRAY['DKOL','DKOS','BSP'], 2.6,1.25,1.2,1.05,'piece',2,80,1.0),
('PNE','Pneumatic Push-In Fitting', ARRAY['Straight','Elbow','Tee','Y-Connector','Male Straight 1/4"','Male Elbow 1/4"'], ARRAY['4mm','6mm','8mm','10mm','12mm'], NULL, 1.4,1.1,1.15,1,'piece',5,100,0.8),
('PNE','Pneumatic PU Tube', ARRAY['4x2.5','6x4','8x5','10x7','12x8'], ARRAY['Blue','Black','Transparent'], ARRAY['25m Roll','100m Roll'], 12,1.35,1.0,3.6,'roll',1,20,0.6),
('WLD','Welding Electrode', ARRAY['Rutile E6013','Basic E7018','Stainless 316L','Cast Iron Ni'], ARRAY['2.0mm','2.5mm','3.2mm','4.0mm'], ARRAY['1kg Pack','5kg Pack'], 6.0,1.8,1.05,4.3,'pack',1,40,1.2),
('WLD','MIG Welding Wire', ARRAY['SG2 Steel','Stainless 308LSi','Aluminium AlMg5','Flux-Cored'], ARRAY['0.6mm','0.8mm','1.0mm','1.2mm'], ARRAY['5kg Spool','15kg Spool'], 18,1.6,1.0,2.7,'spool',1,30,1.0),
('WLD','Welding Gas', ARRAY['Argon 4.6','Argon/CO2 82/18','CO2','Oxygen','Acetylene'], ARRAY['10L Cylinder','20L Cylinder','50L Cylinder'], NULL, 45,1.05,1.6,1,'cylinder',1,20,0.8),
('LUB','Industrial Grease', ARRAY['Lithium EP2','Lithium EP3','Calcium Water-Resistant','High-Temp Complex','Food Grade H1'], ARRAY['400g Cartridge','5kg Pail','18kg Pail','180kg Drum'], NULL, 3.5,1.2,1,1,'unit',1,40,1.0),
('LFT','Round Sling', ARRAY['1t','2t','3t','4t','5t','6t','8t','10t'], ARRAY['1m','2m','3m','4m','6m'], NULL, 6.0,1.3,1.3,1,'piece',1,20,0.6),
('LFT','Ratchet Strap', ARRAY['25mm 800kg','35mm 2000kg','50mm 5000kg','75mm 10000kg'], ARRAY['4m','6m','8m','10m','12m'], NULL, 5.0,1.7,1.12,1,'piece',2,60,1.0),
('LFT','Steel Wire Rope', ARRAY['4mm','6mm','8mm','10mm','12mm','16mm'], ARRAY['Galvanized','Stainless'], ARRAY['per Meter','Reel 100m'], 0.6,1.5,2.4,85,'unit',1,100,0.6),
('LFT','Chain Hoist', ARRAY['0.5t','1t','2t','3t','5t'], ARRAY['3m Lift','6m Lift'], NULL, 95,1.4,1.3,1,'piece',1,4,0.3),
('LOG','Pallet', ARRAY['EUR EPAL 1200x800','Industrial 1200x1000','Half Pallet 800x600','Plastic 1200x800'], ARRAY['New','Used Grade A'], NULL, 14,1.1,0.6,1,'piece',10,500,1.5),
('LOG','Stretch Film', ARRAY['Hand 17µm','Hand 23µm','Machine 17µm','Machine 23µm','Pre-Stretch 8µm'], ARRAY['Black','Transparent'], NULL, 11,1.2,0.95,1,'roll',2,100,1.5),
('LOG','Industrial Absorbent', ARRAY['Universal Pads','Oil-Only Pads','Granules 20kg','Spill Kit 120L','Spill Kit 240L'], NULL, NULL, 25,1.6,1,1,'unit',1,20,0.4);

-- Non-geometric price steps
UPDATE seed_family SET m2 = ARRAY[1, 1.55, 1.9]           WHERE family = 'Cement';
UPDATE seed_family SET m1 = ARRAY[1, 1.8, 9.5, 38]        WHERE family = 'AdBlue';
UPDATE seed_family SET m2 = ARRAY[1, 9, 40]               WHERE family IN ('Hydraulic Oil HLP', 'Formwork Release Oil');
UPDATE seed_family SET m2 = ARRAY[1, 7, 32]               WHERE family = 'Concrete Admixture';
UPDATE seed_family SET m2 = ARRAY[1, 3.6, 30, 140]        WHERE family IN ('Bitumen Emulsion', 'Industrial Degreaser');
UPDATE seed_family SET m2 = ARRAY[1, 3, 3.6, 65]          WHERE family = 'Coolant Antifreeze';
UPDATE seed_family SET m2 = ARRAY[1, 4.6, 165]            WHERE family = 'Windshield Washer Fluid';
UPDATE seed_family SET m2 = ARRAY[1, 3.4, 4.1]            WHERE family = 'Engine Oil';
UPDATE seed_family SET m2 = ARRAY[1, 2.9, 9.5]            WHERE family = 'Engine Oil Bulk';
UPDATE seed_family SET m2 = ARRAY[1, 14, 135]             WHERE family = 'Gear Oil';
UPDATE seed_family SET m2 = ARRAY[1, 3.2, 8, 75]          WHERE family = 'Industrial Grease';
UPDATE seed_family SET m2 = ARRAY[1, 1.7, 3.5]            WHERE family = 'Brake Fluid';
UPDATE seed_family SET m1 = ARRAY[1, 1.25, 1.35, 1.45, 1.3, 1.35, 1.7, 1.8], m2 = ARRAY[1, 1.3, 1.6] WHERE family = 'Car Battery';

CREATE TEMP TABLE seed_product ON COMMIT DROP AS
SELECT row_number() OVER (ORDER BY c.division, c.code, f.fid, a.i1, b.i2, d.i3) AS rn,
       c.division, c.code AS cat_code, c.id AS category_id,
       concat_ws(' ', f.family, NULLIF(a.v1, ''), NULLIF(b.v2, ''), NULLIF(d.v3, '')) AS name,
       GREATEST(0.05, round((f.base
            * COALESCE(f.m1[a.i1], power(f.f1, a.i1 - 1))
            * COALESCE(f.m2[b.i2], power(f.f2, b.i2 - 1))
            * COALESCE(f.m3[d.i3], power(f.f3, d.i3 - 1))
            * (0.94 + random() * 0.12))::numeric, 2)) AS price,
       f.unit, f.qmin, f.qmax,
       -- skewed demand: a few best sellers, a long tail of slow movers
       f.pop * exp(random() * 3 - 1.5) * CASE WHEN random() < 0.03 THEN 5 ELSE 1 END AS pop,
       :'history_start'::timestamptz - interval '40 days' + random() * interval '30 days' AS created,
       NULL::int AS product_id
  FROM seed_family f
  JOIN seed_category c ON c.code = f.cat
 CROSS JOIN LATERAL unnest(f.d1)                     WITH ORDINALITY a(v1, i1)
 CROSS JOIN LATERAL unnest(COALESCE(f.d2, ARRAY[''])) WITH ORDINALITY b(v2, i2)
 CROSS JOIN LATERAL unnest(COALESCE(f.d3, ARRAY[''])) WITH ORDINALITY d(v3, i3);

-- smaller presets keep a random sample of the catalog
DELETE FROM seed_product WHERE random() >= :catalog_share;

-- products that already exist by name are reused, not duplicated
UPDATE seed_product s SET product_id = p."Id" FROM "Products" p WHERE p."Name" = s.name;

WITH ins AS (
    INSERT INTO "Products" ("Uid", "Name", "Price", "CategoryId", "StockQuantity", "Description", "UserId",
                            "Created", "CreatedBy", "LastModified", "LastModifiedBy")
    SELECT gen_random_uuid(), s.name, s.price, s.category_id, 0,
           format('SKU %s-%s-%s · sold per %s', s.division, s.cat_code, lpad(s.rn::text, 5, '0'), s.unit),
           NULL, s.created, u.ids[1], s.created, u.ids[1]
      FROM seed_product s, seed_users u
     WHERE s.product_id IS NULL
     ORDER BY s.rn
    RETURNING "Id", "Name"
)
UPDATE seed_product s SET product_id = ins."Id" FROM ins WHERE ins."Name" = s.name;

CREATE INDEX ON seed_product (product_id);
ANALYZE seed_product;

-- =============================================================================
-- 3. Warehouses (profile decides assortment, size decides order volume)
-- =============================================================================
-- listed in creation order: smaller presets take the first :new_warehouses rows
CREATE TEMP TABLE seed_wh_def (ord serial, name text, address text, contact text, profile text, size numeric) ON COMMIT DROP;
INSERT INTO seed_wh_def (name, address, contact, profile, size) VALUES
    ('Belgrade Central DC',               'Autoput Beograd-Zagreb 35, Beograd',  'Jelena',   'hub',        3.0),
    ('Smederevo River Terminal',          'Lucka 4, Smederevo',                  'Dragan',   'bulk',       1.4),
    ('Kragujevac Auto Parts Center',      'Kralja Petra I 25, Kragujevac',       'Nemanja',  'auto',       1.3),
    ('Pancevo Industrial Depot',          'Spoljnostarcevacka 210, Pancevo',     'Ivana',    'industrial', 1.1),
    ('Novi Sad Distribution Center',      'Bulevar oslobodjenja 102, Novi Sad',  'Marko',    'regional',   1.3),
    ('Kraljevo Building Materials Yard',  'Ibarska 88, Kraljevo',                'Milan',    'bulk',       1.0),
    ('Sabac Auto & Truck Parts',          'Pocerska 140, Sabac',                 'Stefan',   'auto',       0.9),
    ('Subotica Regional Warehouse',       'Segedinski put 76, Subotica',         'Tamara',   'regional',   0.9),
    ('Zrenjanin Regional Warehouse',      'Novosadski put 12, Zrenjanin',        'Bojan',    'regional',   0.8),
    ('Cacak Regional Warehouse',          'Ljubicska 22, Cacak',                 'Milica',   'regional',   0.9),
    ('Uzice Regional Warehouse',          'Nikole Pasica 51, Uzice',             'Vladimir', 'regional',   0.7),
    ('Leskovac Regional Warehouse',       'Bulevar oslobodjenja 220, Leskovac',  'Ana',      'regional',   0.8),
    ('Valjevo Regional Warehouse',        'Karadjordjeva 150, Valjevo',          'Petar',    'regional',   0.7);

INSERT INTO "Warehouses" ("Uid", "Name", "Address", "ContactPerson", "Company",
                          "Created", "CreatedBy", "LastModified", "LastModifiedBy")
SELECT gen_random_uuid(), w.name, w.address, w.contact, 'Inventory inc', t, u.ids[1], t, u.ids[1]
  FROM seed_wh_def w, seed_users u, (SELECT :'history_start'::timestamptz - interval '40 days' AS t) x
 WHERE w.ord <= :new_warehouses
   AND NOT EXISTS (SELECT 1 FROM "Warehouses" e WHERE e."Name" = w.name);

-- every warehouse takes part; ones not defined above get a profile here
CREATE TEMP TABLE seed_wh ON COMMIT DROP AS
SELECT e."Id" AS wh_id,
       COALESCE(d.profile, CASE e."Name" WHEN 'Main Warehouse' THEN 'hub' ELSE 'regional' END) AS profile,
       COALESCE(d.size,    CASE e."Name" WHEN 'Main Warehouse' THEN 2.0 ELSE 0.8 END)          AS size,
       :'history_start'::timestamptz - interval '7 days' + interval '7 hours' + random() * interval '5 days' AS opening_at
  FROM "Warehouses" e LEFT JOIN seed_wh_def d ON d.name = e."Name";

-- share of each division's catalog a warehouse profile carries
CREATE TEMP TABLE seed_coverage (profile text, division text, coverage numeric) ON COMMIT DROP;
INSERT INTO seed_coverage VALUES
    ('hub','HW',.92),        ('hub','BLK',.75),        ('hub','AUT',.85),        ('hub','IND',.85),
    ('regional','HW',.55),   ('regional','BLK',.35),   ('regional','AUT',.40),   ('regional','IND',.25),
    ('bulk','HW',.10),       ('bulk','BLK',.95),       ('bulk','AUT',.05),       ('bulk','IND',.15),
    ('auto','HW',.15),       ('auto','BLK',.02),       ('auto','AUT',.95),       ('auto','IND',.45),
    ('industrial','HW',.45), ('industrial','BLK',.20), ('industrial','AUT',.25), ('industrial','IND',.95);

-- =============================================================================
-- 4. Assortment: which products each warehouse carries (+ cumulative demand
--    weights for weighted random picking; lot = replenishment batch, set in 6)
-- =============================================================================
CREATE TEMP TABLE seed_assort ON COMMIT DROP AS
SELECT x.wh_id, x.product_id, x.division, x.category_id, x.pop, x.qmin, x.qmax, x.qmin AS lot,
       sum(x.pop) OVER (PARTITION BY x.wh_id, x.division ORDER BY x.product_id) - x.pop AS lo
  FROM (
    -- random() lives in the select list so it is drawn per warehouse x product
    SELECT w.wh_id, p.product_id, p.division, p.category_id, p.pop, p.qmin, p.qmax, c.coverage,
           random() AS r
      FROM seed_wh w
      JOIN seed_coverage c ON c.profile = w.profile
      JOIN seed_product p  ON p.division = c.division
  ) x
 WHERE x.r < x.coverage;
CREATE INDEX ON seed_assort (wh_id, division, lo);
CREATE UNIQUE INDEX ON seed_assort (wh_id, product_id);
ANALYZE seed_assort;

CREATE TEMP TABLE seed_wh_div ON COMMIT DROP AS
SELECT wh_id, division, total,
       sum(total) OVER (PARTITION BY wh_id ORDER BY division) - total AS lo,
       sum(total) OVER (PARTITION BY wh_id)                          AS wh_total
  FROM (SELECT wh_id, division, sum(pop) AS total FROM seed_assort GROUP BY 1, 2) t;

-- =============================================================================
-- 5. Outgoing orders (customer shipments): weekdays, Serbian public holidays
--    off, summer construction peak, ~40% business growth over the period
-- =============================================================================
CREATE TEMP TABLE seed_out_order ON COMMIT DROP AS
WITH days AS (
    SELECT d::date AS day,
           (d::date - :'history_start'::date)::numeric / (:'history_end'::date - :'history_start'::date) AS progress
      FROM generate_series(:'history_start'::date, :'history_end'::date, interval '1 day') d
), factor AS (
    SELECT w.wh_id, w.size, d.day,
           CASE extract(isodow FROM d.day) WHEN 7 THEN 0 WHEN 6 THEN 0.3 ELSE 1 END
         * CASE WHEN to_char(d.day, 'MM-DD') IN ('01-01','01-02','01-07','02-15','02-16','05-01','05-02','11-11') THEN 0 ELSE 1 END
         * (1 + 0.3 * sin(2 * pi() * (extract(doy FROM d.day) - 100) / 365.0))
         * (0.8 + 0.4 * d.progress) AS f
      FROM seed_wh w CROSS JOIN days d
)
SELECT ok, wh_id, created, extract(epoch FROM (:'now_ts'::timestamptz - created)) / 86400 AS age,
       r_div, r_status, r_lines, division, status, done_at, n_lines
  FROM (
SELECT nextval('seed_ok') AS ok, f.wh_id,
       (f.day + time '07:00' + random() * interval '10 hours')::timestamptz AS created,
       random() AS r_div, random() AS r_status, random() AS r_lines,
       NULL::text AS division, NULL::int AS status, NULL::timestamptz AS done_at, NULL::int AS n_lines
  FROM factor f
 CROSS JOIN LATERAL generate_series(1, floor(:orders_per_day * f.size * f.f * (0.75 + random() * 0.5) + random())::int) g
 WHERE f.f > 0
  ) t;

UPDATE seed_out_order o
   SET division = (SELECT d.division FROM seed_wh_div d
                    WHERE d.wh_id = o.wh_id AND d.lo <= o.r_div * d.wh_total
                    ORDER BY d.lo DESC LIMIT 1);

-- Older orders are settled; the last 3 weeks still have open (Pending) ones.
DELETE FROM seed_out_order WHERE division IS NULL;  -- warehouse carries nothing

UPDATE seed_out_order o
   SET status = CASE
           WHEN o.age > 21 THEN CASE WHEN o.r_status < 0.955 THEN 3 WHEN o.r_status < 0.995 THEN 4 ELSE 2 END
           ELSE CASE WHEN o.r_status < 0.04 THEN 4 WHEN o.r_status < 0.25 + 0.7 * o.age / 21 THEN 3 ELSE 2 END
       END,
       n_lines = LEAST(25, 1 + floor(-ln(1 - o.r_lines) * dv.lines_per_order))::int
  FROM seed_division dv
 WHERE dv.code = o.division;

UPDATE seed_out_order
   SET done_at = CASE status
           WHEN 3 THEN created + interval '20 minutes' + power(random(), 2) * interval '30 hours'
           WHEN 4 THEN created + random() * interval '48 hours'
           ELSE created END;

-- whatever would finish in the future is still Pending today
UPDATE seed_out_order SET status = 2, done_at = created WHERE done_at > :'now_ts'::timestamptz;

-- Lines: weighted pick of products this warehouse carries in the order's division
CREATE TEMP TABLE seed_out_pick ON COMMIT DROP AS
SELECT o.ok, o.wh_id, o.division, random() * d.total AS r
  FROM seed_out_order o
  JOIN seed_wh_div d ON d.wh_id = o.wh_id AND d.division = o.division
 CROSS JOIN LATERAL generate_series(1, o.n_lines) g;

CREATE TEMP TABLE seed_out_line ON COMMIT DROP AS
SELECT p.ok, a.product_id,
       -- same product picked twice in one order -> quantities merge (AddItem rule)
       sum(a.qmin + floor(power(random(), 2.2) * (a.qmax - a.qmin + 1)))::int AS qty
  FROM seed_out_pick p
 CROSS JOIN LATERAL (
        SELECT s.product_id, s.qmin, s.qmax FROM seed_assort s
         WHERE s.wh_id = p.wh_id AND s.division = p.division AND s.lo <= p.r
         ORDER BY s.lo DESC LIMIT 1) a
 GROUP BY p.ok, a.product_id;
CREATE INDEX ON seed_out_line (ok);
ANALYZE seed_out_order; ANALYZE seed_out_line;

-- =============================================================================
-- 6. Incoming orders (supplier deliveries) derived from demand so that stock
--    never goes negative:
--    - lot = 4-10 weeks of that product's demand in that warehouse
--    - every product gets an opening delivery of one lot before history starts
--    - completed outgoing quantities are split into lots; lot k is delivered
--      1-12 days before the first shipment that needs it
-- =============================================================================
UPDATE seed_assort a
   SET lot = GREATEST(a.qmin, ceil(d.qty / ((:'history_end'::date - :'history_start'::date) / 7.0)
                                   * (4 + random() * 6)))::int
  FROM (SELECT o.wh_id, l.product_id, sum(l.qty) AS qty
          FROM seed_out_line l JOIN seed_out_order o ON o.ok = l.ok
         WHERE o.status <> 4
         GROUP BY 1, 2) d
 WHERE d.wh_id = a.wh_id AND d.product_id = a.product_id;

CREATE TEMP TABLE seed_out_move ON COMMIT DROP AS
SELECT o.wh_id, l.product_id, o.done_at, l.qty,
       sum(l.qty) OVER (PARTITION BY o.wh_id, l.product_id ORDER BY o.done_at, o.ok) AS cum
  FROM seed_out_line l JOIN seed_out_order o ON o.ok = l.ok
 WHERE o.status = 3;

CREATE TEMP TABLE seed_receipt ON COMMIT DROP AS
SELECT a.wh_id, a.product_id, a.category_id, a.lot AS qty,
       GREATEST(w.opening_at,
                date_trunc('day', b.need_at - (1 + random() * 11) * interval '1 day')
                + interval '7 hours' + random() * interval '8 hours') AS arrival
  FROM (SELECT m.wh_id, m.product_id, k, min(m.done_at) AS need_at
          FROM seed_out_move m
          JOIN seed_assort a USING (wh_id, product_id)
         CROSS JOIN LATERAL generate_series((m.cum - m.qty) / a.lot + 1, ceil(m.cum::numeric / a.lot)::bigint) k
         GROUP BY 1, 2, 3) b
  JOIN seed_assort a USING (wh_id, product_id)
  JOIN seed_wh w USING (wh_id)
UNION ALL
SELECT a.wh_id, a.product_id, a.category_id, a.lot, w.opening_at   -- opening / safety stock
  FROM seed_assort a JOIN seed_wh w USING (wh_id);

-- one supplier delivery per warehouse + category + week, max 40 lines each
CREATE TEMP TABLE seed_in_line ON COMMIT DROP AS
SELECT wh_id, category_id, wk, product_id, qty, arrival,
       (row_number() OVER (PARTITION BY wh_id, category_id, wk ORDER BY product_id) - 1) / 40 AS chunk
  FROM (SELECT wh_id, category_id, date_trunc('week', arrival) AS wk, product_id,
               sum(qty)::int AS qty, min(arrival) AS arrival
          FROM seed_receipt GROUP BY 1, 2, 3, 4) t;

CREATE TEMP TABLE seed_in_order ON COMMIT DROP AS
SELECT nextval('seed_ok') AS ok, wh_id, category_id, wk, chunk,
       min(arrival) AS done_at,
       min(arrival) - (2 + random() * 8) * interval '1 day' AS created
  FROM seed_in_line GROUP BY wh_id, category_id, wk, chunk;

-- =============================================================================
-- 7. All orders + lines
-- =============================================================================
CREATE TEMP TABLE seed_order_all (
    ok bigint PRIMARY KEY, wh_id int, type int, status int,
    created timestamptz, modified timestamptz, uid uuid DEFAULT gen_random_uuid(), by_user text
) ON COMMIT DROP;
CREATE TEMP TABLE seed_line_all (ok bigint, product_id int, qty int) ON COMMIT DROP;

-- outgoing (type 2)
INSERT INTO seed_order_all (ok, wh_id, type, status, created, modified)
SELECT ok, wh_id, 2, status, created, done_at FROM seed_out_order;
INSERT INTO seed_line_all SELECT ok, product_id, qty FROM seed_out_line;

-- completed incoming (type 1)
INSERT INTO seed_order_all (ok, wh_id, type, status, created, modified)
SELECT ok, wh_id, 1, 3, created, done_at FROM seed_in_order;
INSERT INTO seed_line_all
SELECT o.ok, l.product_id, l.qty
  FROM seed_in_line l
  JOIN seed_in_order o USING (wh_id, category_id, wk, chunk);

-- cancelled incoming: ~2.5% of supplier orders were cancelled and re-placed
WITH src AS (
    SELECT ok AS src_ok, nextval('seed_ok') AS ok, wh_id, created - interval '3 days' AS created
      FROM seed_in_order
     WHERE random() < 0.025 AND created - interval '3 days' > :'history_start'::timestamptz
), ins AS (
    INSERT INTO seed_order_all (ok, wh_id, type, status, created, modified)
    SELECT ok, wh_id, 1, 4, created, created + random() * interval '2 days' FROM src
    RETURNING ok
)
INSERT INTO seed_line_all
SELECT s.ok, l.product_id, l.qty FROM src s JOIN seed_line_all l ON l.ok = s.src_ok;

-- open purchase orders (Pending incoming) placed during the last two weeks
WITH po AS (
    SELECT nextval('seed_ok') AS ok, wh_id, category_id,
           :'now_ts'::timestamptz - random() * interval '14 days' AS created,
           1 + floor(random() * 8)::int AS n
      FROM (SELECT DISTINCT wh_id, category_id FROM seed_assort) t
     WHERE random() < 0.35
), ins AS (
    INSERT INTO seed_order_all (ok, wh_id, type, status, created, modified)
    SELECT ok, wh_id, 1, 2, created, created FROM po
    RETURNING ok
)
INSERT INTO seed_line_all
SELECT po.ok, x.product_id, x.lot
  FROM po
 CROSS JOIN LATERAL (SELECT a.product_id, a.lot FROM seed_assort a
                      WHERE a.wh_id = po.wh_id AND a.category_id = po.category_id
                      ORDER BY random() LIMIT po.n) x;

UPDATE seed_order_all o
   SET by_user = u.ids[1 + floor(random() * cardinality(u.ids))::int]
  FROM seed_users u
 WHERE cardinality(u.ids) > 0;

-- item price = product price at the time the order was placed (~6% per year)
CREATE TEMP TABLE seed_line_priced ON COMMIT DROP AS
SELECT l.ok, l.product_id, l.qty,
       GREATEST(0.01, round((p.price / power(1.06,
           extract(epoch FROM (:'now_ts'::timestamptz - o.created)) / 31557600.0))::numeric, 2)) AS price
  FROM seed_line_all l
  JOIN seed_order_all o ON o.ok = l.ok
  JOIN seed_product p   ON p.product_id = l.product_id;
CREATE INDEX ON seed_line_priced (ok);

-- =============================================================================
-- 8. Write Orders and OrderItems (Ids follow chronological order)
-- =============================================================================
CREATE TEMP TABLE seed_order_map (ok bigint PRIMARY KEY, id int) ON COMMIT DROP;

WITH totals AS (
    SELECT ok, sum(price * qty) AS total FROM seed_line_priced GROUP BY ok
), ins AS (
    INSERT INTO "Orders" ("Uid", "Status", "Type", "WarehouseId", "TotalPrice",
                          "Created", "CreatedBy", "LastModified", "LastModifiedBy")
    SELECT o.uid, o.status, o.type, o.wh_id, COALESCE(t.total, 0),
           o.created, o.by_user, o.modified, o.by_user
      FROM seed_order_all o LEFT JOIN totals t USING (ok)
     ORDER BY o.created, o.ok
    RETURNING "Id", "Uid"
)
INSERT INTO seed_order_map
SELECT o.ok, ins."Id" FROM ins JOIN seed_order_all o ON o.uid = ins."Uid";

INSERT INTO "OrderItems" ("Uid", "OrderId", "ProductId", "Quantity", "Price",
                          "Created", "CreatedBy", "LastModified", "LastModifiedBy")
SELECT gen_random_uuid(), m.id, l.product_id, l.qty, l.price,
       o.created, o.by_user, o.created, o.by_user
  FROM seed_line_priced l
  JOIN seed_order_map m ON m.ok = l.ok
  JOIN seed_order_all o ON o.ok = l.ok
 ORDER BY m.id, l.product_id;

-- =============================================================================
-- 9. Stocks = existing + completed incoming - completed outgoing
-- =============================================================================
CREATE TEMP TABLE seed_stock ON COMMIT DROP AS
SELECT o.wh_id, l.product_id,
       sum(CASE o.type WHEN 1 THEN l.qty ELSE -l.qty END) AS delta,
       min(o.modified) AS first_at, max(o.modified) AS last_at
  FROM seed_line_all l JOIN seed_order_all o ON o.ok = l.ok
 WHERE o.status = 3
 GROUP BY 1, 2;

UPDATE "Stocks" s
   SET "Quantity" = s."Quantity" + x.delta, "LastModified" = x.last_at
  FROM seed_stock x
 WHERE s."WarehouseId" = x.wh_id AND s."ProductId" = x.product_id;

INSERT INTO "Stocks" ("Uid", "ProductId", "WarehouseId", "Quantity",
                      "Created", "CreatedBy", "LastModified", "LastModifiedBy")
SELECT gen_random_uuid(), x.product_id, x.wh_id, x.delta, x.first_at, u.ids[1], x.last_at, u.ids[1]
  FROM seed_stock x, seed_users u
 WHERE NOT EXISTS (SELECT 1 FROM "Stocks" s WHERE s."WarehouseId" = x.wh_id AND s."ProductId" = x.product_id)
 ORDER BY x.first_at;

-- Product.StockQuantity of seeded products = total stock over all warehouses
UPDATE "Products" p
   SET "StockQuantity" = COALESCE((SELECT sum(s."Quantity") FROM "Stocks" s WHERE s."ProductId" = p."Id"), 0)
 WHERE p."Id" IN (SELECT product_id FROM seed_product);

COMMIT;

ANALYZE "Categories"; ANALYZE "Products"; ANALYZE "Warehouses";
ANALYZE "Stocks"; ANALYZE "Orders"; ANALYZE "OrderItems";

SELECT 'Categories' AS "table", count(*) FROM "Categories"
UNION ALL SELECT 'Warehouses', count(*) FROM "Warehouses"
UNION ALL SELECT 'Products',   count(*) FROM "Products"
UNION ALL SELECT 'Stocks',     count(*) FROM "Stocks"
UNION ALL SELECT 'Orders',     count(*) FROM "Orders"
UNION ALL SELECT 'OrderItems', count(*) FROM "OrderItems";
