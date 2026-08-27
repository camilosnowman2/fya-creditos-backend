-- Datos de prueba: los 10 registros del Anexo del examen técnico (nombre,
-- monto prestado, plazo e interés). El anexo no incluye cédula ni comercial,
-- así que esos dos campos se rellenan aquí con valores ficticios solo para
-- poder probar los filtros y el orden de /api/creditos.

INSERT INTO creditos (id, nombre_cliente, cedula, valor_credito, tasa_interes, plazo_meses, nombre_comercial, fecha_registro)
VALUES
    (gen_random_uuid(), 'Pepito Perez',       '1000000001', 7800000.00,  2.000, 10, 'Comercial Uno',   now() - interval '9 days'),
    (gen_random_uuid(), 'Maria Perez',        '1000000002', 12500000.00, 2.000, 5,  'Comercial Dos',   now() - interval '8 days'),
    (gen_random_uuid(), 'Antonio Rodriguez',  '1000000003', 10312673.00, 2.000, 5,  'Comercial Tres',  now() - interval '7 days'),
    (gen_random_uuid(), 'Giselle Lopez',      '1000000004', 8628510.00,  2.000, 12, 'Comercial Uno',   now() - interval '6 days'),
    (gen_random_uuid(), 'Martha Perez',       '1000000005', 5889085.00,  2.000, 24, 'Comercial Dos',   now() - interval '5 days'),
    (gen_random_uuid(), 'Isaac Llanos',       '1000000006', 14793565.00, 2.000, 48, 'Comercial Tres',  now() - interval '4 days'),
    (gen_random_uuid(), 'Teresa Gutierrez',   '1000000007', 8072348.00,  2.000, 50, 'Comercial Uno',   now() - interval '3 days'),
    (gen_random_uuid(), 'Isabel Llanos',      '1000000008', 5143860.00,  2.000, 60, 'Comercial Dos',   now() - interval '2 days'),
    (gen_random_uuid(), 'Paola Tao',          '1000000009', 12881963.00, 2.000, 24, 'Comercial Tres',  now() - interval '1 days'),
    (gen_random_uuid(), 'Wendy Moscoso',      '1000000010', 13484682.00, 2.000, 40, 'Comercial Uno',   now())
ON CONFLICT (id) DO NOTHING;
