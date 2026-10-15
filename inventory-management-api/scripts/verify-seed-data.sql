-- =============================================================================
-- Checks the business rules over the whole database. Read-only.
--   psql -h localhost -p 5433 -U postgres -d inventory_management -f scripts/verify-seed-data.sql
-- Every "violations" row should be 0.
-- =============================================================================

\echo '=== Row counts'
SELECT 'Categories' AS "table", count(*) FROM "Categories"
UNION ALL SELECT 'Warehouses', count(*) FROM "Warehouses"
UNION ALL SELECT 'Products',   count(*) FROM "Products"
UNION ALL SELECT 'Stocks',     count(*) FROM "Stocks"
UNION ALL SELECT 'Orders',     count(*) FROM "Orders"
UNION ALL SELECT 'OrderItems', count(*) FROM "OrderItems";

\echo '=== Violations (all must be 0)'
WITH item_totals AS (
    SELECT "OrderId", sum("Price" * "Quantity") AS total, count(*) AS lines
      FROM "OrderItems" GROUP BY "OrderId"
),
-- every completed stock movement, signed (+ incoming / - outgoing)
moves AS (
    SELECT o."Id" AS order_id, o."WarehouseId", i."ProductId", o."LastModified", o."Type",
           CASE o."Type" WHEN 1 THEN i."Quantity" ELSE -i."Quantity" END AS delta
      FROM "OrderItems" i
      JOIN "Orders" o   ON o."Id" = i."OrderId" AND o."Status" = 3
      JOIN "Products" p ON p."Id" = i."ProductId" AND p."Description" LIKE 'SKU %'   -- seeded products
),
balance AS (
    SELECT "WarehouseId", "ProductId", sum(delta) AS qty FROM moves GROUP BY 1, 2
)
SELECT 'order total <> sum(items)' AS rule, count(*) AS violations
  FROM "Orders" o LEFT JOIN item_totals t ON t."OrderId" = o."Id"
 WHERE o."TotalPrice" <> COALESCE(t.total, 0)
UNION ALL
SELECT 'completed order without items', count(*)
  FROM "Orders" o LEFT JOIN item_totals t ON t."OrderId" = o."Id"
 WHERE o."Status" = 3 AND t."OrderId" IS NULL
UNION ALL
SELECT 'same product twice in one order', count(*)
  FROM (SELECT 1 FROM "OrderItems" GROUP BY "OrderId", "ProductId" HAVING count(*) > 1) x
UNION ALL
SELECT 'item quantity <= 0', count(*) FROM "OrderItems" WHERE "Quantity" <= 0
UNION ALL
SELECT 'unknown status / type', count(*) FROM "Orders" WHERE "Status" NOT IN (2, 3, 4) OR "Type" NOT IN (1, 2)
UNION ALL
SELECT 'duplicate stock rows', count(*)
  FROM (SELECT 1 FROM "Stocks" GROUP BY "WarehouseId", "ProductId" HAVING count(*) > 1) x
UNION ALL
SELECT 'negative stock', count(*) FROM "Stocks" WHERE "Quantity" < 0
UNION ALL
-- replay completed movements in time order (deliveries first on ties):
-- stock may never dip below zero
SELECT 'stock below zero at some point (seeded products)', count(*)
  FROM (SELECT sum(delta) OVER (PARTITION BY "WarehouseId", "ProductId"
                                ORDER BY "LastModified", "Type", order_id) AS running
          FROM moves) r
 WHERE r.running < 0
UNION ALL
SELECT 'stock <> completed in - out (seeded products)', count(*)
  FROM balance b
  FULL JOIN (SELECT s.* FROM "Stocks" s JOIN "Products" p ON p."Id" = s."ProductId" AND p."Description" LIKE 'SKU %') s
         ON s."WarehouseId" = b."WarehouseId" AND s."ProductId" = b."ProductId"
 WHERE COALESCE(s."Quantity", 0) <> COALESCE(b.qty, 0);

\echo '=== Orders per year, type and status (Type 1=Incoming 2=Outgoing, Status 2=Pending 3=Completed 4=Cancelled)'
SELECT extract(year FROM "Created")::int AS year, "Type", "Status", count(*) AS orders,
       round(sum("TotalPrice")) AS value
  FROM "Orders" GROUP BY 1, 2, 3 ORDER BY 1, 2, 3;

\echo '=== Warehouses'
SELECT w."Id", w."Name", s.products_in_stock, s.units, o.orders
  FROM "Warehouses" w
  LEFT JOIN (SELECT "WarehouseId", count(*) FILTER (WHERE "Quantity" > 0) AS products_in_stock,
                    sum("Quantity") AS units
               FROM "Stocks" GROUP BY 1) s ON s."WarehouseId" = w."Id"
  LEFT JOIN (SELECT "WarehouseId", count(*) AS orders FROM "Orders" GROUP BY 1) o ON o."WarehouseId" = w."Id"
 ORDER BY w."Id";
