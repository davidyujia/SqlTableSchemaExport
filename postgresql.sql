SELECT * FROM information_schema.tables WHERE table_schema = 'public' AND table_name NOT LIKE '%_ref_sys' AND table_type <> 'VIEW';

SELECT * FROM information_schema.columns WHERE table_name ='road_event_standard';


SELECT c.relname As tname, CASE WHEN c.relkind = 'v' THEN 'view' ELSE 'table' END As type,
    pg_get_userbyid(c.relowner) AS towner, t.spcname AS tspace,
    n.nspname AS sname,  d.description
   FROM pg_class As c
   LEFT JOIN pg_namespace n ON n.oid = c.relnamespace
   LEFT JOIN pg_tablespace t ON t.oid = c.reltablespace
   LEFT JOIN pg_description As d ON (d.objoid = c.oid AND d.objsubid = 0)
   WHERE c.relkind IN('r', 'v') AND d.description > ''
   ORDER BY n.nspname, c.relname;


SELECT a.attname As column_name,  d.description
   FROM pg_class As c
    INNER JOIN pg_attribute As a ON c.oid = a.attrelid
   LEFT JOIN pg_namespace n ON n.oid = c.relnamespace
   LEFT JOIN pg_tablespace t ON t.oid = c.reltablespace
   LEFT JOIN pg_description As d ON (d.objoid = c.oid AND d.objsubid = a.attnum)
   WHERE  c.relkind IN('r', 'v') AND  n.nspname = 'public' AND c.relname = 'road_event_standard'
   ORDER BY n.nspname, c.relname, a.attname;
