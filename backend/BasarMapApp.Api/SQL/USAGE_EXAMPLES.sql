-- ===================================================================
-- GIS Boundary API - PostgreSQL Kullanım Örnekleri
-- ===================================================================
-- Bu dosya, veritabanı seviyesinde GIS sorgularını test etmek için
-- örnek SQL komutları içerir.
-- NOT: Tablolar ogc_fid (PostGIS/OGR import standardı) primary key kullanır
-- ===================================================================

-- 1. Tablo Yapılarını Kontrol Etme
-- ===================================================================

-- Tablo sütunlarını listele
SELECT 
    table_name, 
    column_name, 
    data_type,
    udt_name
FROM information_schema.columns
WHERE table_name IN ('ref_provinces', 'ref_districts', 'ref_settlements')
ORDER BY table_name, ordinal_position;

-- Geometri tiplerini kontrol et
SELECT 
    f_table_name as table_name,
    f_geometry_column as geom_column,
    coord_dimension,
    srid,
    type
FROM geometry_columns
WHERE f_table_name IN ('ref_provinces', 'ref_districts', 'ref_settlements');


-- 2. İndeks Kontrolü
-- ===================================================================

-- GIST indekslerini listele
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename IN ('ref_provinces', 'ref_districts', 'ref_settlements')
ORDER BY tablename, indexname;


-- 3. Temel Veri Sorgulama
-- ===================================================================

-- Tüm illeri listele (isim sırasına göre)
SELECT 
    ogc_fid,
    name,
    plate_code,
    ST_GeometryType(geom) as geom_type,
    ST_SRID(geom) as srid
FROM ref_provinces
ORDER BY name;

-- Belirli bir ile ait ilçeleri getir (örn: İstanbul - plate_code = 34)
SELECT 
    d.ogc_fid,
    d.name as district_name,
    p.name as province_name,
    p.plate_code
FROM ref_districts d
INNER JOIN ref_provinces p ON d.province_id = p.ogc_fid
WHERE p.plate_code = 34
ORDER BY d.name;

-- İlçeye ait yerleşim yerlerini getir
SELECT 
    s.ogc_fid,
    s.name as settlement_name,
    s.settlement_type,
    s.population,
    d.name as district_name,
    ST_X(s.geom) as longitude,
    ST_Y(s.geom) as latitude
FROM ref_settlements s
INNER JOIN ref_districts d ON s.district_id = d.ogc_fid
WHERE d.ogc_fid = 1
LIMIT 20;


-- 4. GeoJSON Dönüşümü Testleri
-- ===================================================================

-- İl geometrisini GeoJSON olarak al
SELECT 
    ogc_fid,
    name,
    ST_AsGeoJSON(geom)::json as geometry
FROM ref_provinces
WHERE plate_code = 34; -- İstanbul

-- İlçe geometrisini GeoJSON olarak al (simplified)
SELECT 
    ogc_fid,
    name,
    ST_AsGeoJSON(ST_SimplifyPreserveTopology(geom, 0.001))::json as geometry_simplified
FROM ref_districts
WHERE ogc_fid = 1;


-- 5. Basitleştirme (Simplification) Testleri
-- ===================================================================

-- Orijinal vs Basitleştirilmiş vertex sayısını karşılaştır
SELECT 
    name,
    ST_NPoints(geom) as original_points,
    ST_NPoints(ST_SimplifyPreserveTopology(geom, 0.001)) as simplified_points,
    ROUND(
        (1 - ST_NPoints(ST_SimplifyPreserveTopology(geom, 0.001))::numeric / ST_NPoints(geom)::numeric) * 100, 
        2
    ) as reduction_percentage
FROM ref_provinces
ORDER BY reduction_percentage DESC
LIMIT 10;


-- 6. Bounding Box (Spatial Query) Testleri
-- ===================================================================

-- İstanbul bölgesindeki yerleşim yerlerini bul (Bounding Box)
-- Koordinatlar: minLon=28.5, minLat=40.8, maxLon=29.5, maxLat=41.3
SELECT 
    s.ogc_fid,
    s.name,
    s.settlement_type,
    d.name as district_name,
    ST_X(s.geom) as lon,
    ST_Y(s.geom) as lat
FROM ref_settlements s
INNER JOIN ref_districts d ON s.district_id = d.ogc_fid
WHERE ST_Intersects(
    s.geom,
    ST_MakeEnvelope(28.5, 40.8, 29.5, 41.3, 4326)
);

-- Ankara merkezindeki yerleşim yerleri
SELECT 
    COUNT(*) as settlement_count,
    AVG(population) as avg_population
FROM ref_settlements
WHERE ST_Intersects(
    geom,
    ST_MakeEnvelope(32.5, 39.7, 33.0, 40.0, 4326)
);


-- 7. Performance Test Sorguları
-- ===================================================================

-- EXPLAIN ANALYZE ile sorgu performansını test et (GIST indeks kullanımını gösterir)
EXPLAIN ANALYZE
SELECT ogc_fid, name, ST_AsGeoJSON(geom)
FROM ref_settlements
WHERE ST_Intersects(
    geom,
    ST_MakeEnvelope(28.9, 41.0, 29.1, 41.1, 4326)
);


-- 8. İstatistiksel Bilgiler
-- ===================================================================

-- İl başına ilçe sayısı
SELECT 
    p.name as province_name,
    COUNT(d.ogc_fid) as district_count
FROM ref_provinces p
LEFT JOIN ref_districts d ON p.ogc_fid = d.province_id
GROUP BY p.ogc_fid, p.name
ORDER BY district_count DESC;

-- İlçe başına yerleşim yeri sayısı ve toplam nüfus
SELECT 
    d.name as district_name,
    p.name as province_name,
    COUNT(s.ogc_fid) as settlement_count,
    SUM(s.population) as total_population
FROM ref_districts d
INNER JOIN ref_provinces p ON d.province_id = p.ogc_fid
LEFT JOIN ref_settlements s ON d.ogc_fid = s.district_id
GROUP BY d.ogc_fid, d.name, p.name
ORDER BY total_population DESC NULLS LAST
LIMIT 20;


-- 9. Geometri Validasyon
-- ===================================================================

-- Geçersiz geometrileri kontrol et
SELECT 
    'ref_provinces' as table_name,
    ogc_fid,
    name,
    ST_IsValid(geom) as is_valid,
    ST_IsValidReason(geom) as invalid_reason
FROM ref_provinces
WHERE NOT ST_IsValid(geom)
UNION ALL
SELECT 
    'ref_districts' as table_name,
    ogc_fid,
    name,
    ST_IsValid(geom) as is_valid,
    ST_IsValidReason(geom) as invalid_reason
FROM ref_districts
WHERE NOT ST_IsValid(geom);


-- 10. Mesafe Hesaplamaları
-- ===================================================================

-- Ankara merkez ile diğer il merkezleri arası mesafe (km)
WITH ankara_center AS (
    SELECT geom FROM ref_settlements WHERE ogc_fid = 1 -- Ankara merkez settlement ogc_fid
)
SELECT 
    p.name as province_name,
    ROUND(
        ST_Distance(
            (SELECT geom FROM ankara_center),
            ST_Centroid(p.geom)::geography
        ) / 1000, 
        2
    ) as distance_km
FROM ref_provinces p
ORDER BY distance_km
LIMIT 10;


-- ===================================================================
-- NOTLAR
-- ===================================================================
-- 
-- 1. ST_SimplifyPreserveTopology: Topology bozulmadan basitleştirme
--    - Tolerans 0.001 ≈ 100 metre (SRID 4326 için)
--    - Daha düşük tolerans = daha detaylı
--
-- 2. ST_Intersects: GIST indeks kullanır, çok hızlı
--    - Bounding Box sorguları için ideal
--
-- 3. ST_AsGeoJSON: Direkt GeoJSON çıktısı
--    - Frontend entegrasyonu için mükemmel
--
-- 4. ST_Distance: Mesafe hesaplama
--    - geography tipinde metre cinsinden
--    - geometry tipinde SRID biriminde
--
-- 5. EXPLAIN ANALYZE: Query performansını analiz et
--    - "Index Scan using idx_*_geom_gist" görüyorsanız ✓
--    - "Seq Scan" görüyorsanız indeks eksik veya kullanılmıyor ✗
--
-- ===================================================================
