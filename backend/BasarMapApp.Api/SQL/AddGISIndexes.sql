-- ===================================================================
-- GIS Performance Optimization - GIST Spatial Indexes
-- ===================================================================
-- Bu script, referans GIS verilerine yüksek performanslı spatial sorgular
-- için GIST indeksleri ekler.
-- NOT: Tablolar ogc_fid (PostGIS/OGR import standardı) primary key kullanır
-- ===================================================================

-- 1. İl (Province) tablosu için GIST indeks
-- Poligon geometrileri üzerinde hızlı spatial sorgular için
CREATE INDEX IF NOT EXISTS idx_ref_provinces_geom_gist 
ON ref_provinces USING GIST (geom);

-- 2. İlçe (District) tablosu için GIST indeks
-- İlçe sınırları üzerinde hızlı spatial sorgular için
CREATE INDEX IF NOT EXISTS idx_ref_districts_geom_gist 
ON ref_districts USING GIST (geom);

-- 3. Yerleşim Yerleri (Settlement) tablosu için GIST indeks
-- Nokta geometrileri üzerinde hızlı spatial sorgular için
CREATE INDEX IF NOT EXISTS idx_ref_settlements_geom_gist 
ON ref_settlements USING GIST (geom);

-- ===================================================================
-- Ek Performans İyileştirmeleri
-- ===================================================================

-- İlçelerin il_id foreign key için B-tree indeks (hiyerarşik sorgular için)
CREATE INDEX IF NOT EXISTS idx_ref_districts_province_id 
ON ref_districts (province_id);

-- Yerleşim yerlerinin ilçe_id foreign key için B-tree indeks
CREATE INDEX IF NOT EXISTS idx_ref_settlements_district_id 
ON ref_settlements (district_id);

-- Vacuum analyze ile istatistikleri güncelle
VACUUM ANALYZE ref_provinces;
VACUUM ANALYZE ref_districts;
VACUUM ANALYZE ref_settlements;

-- ===================================================================
-- İndeks Bilgilerini Kontrol Etme
-- ===================================================================
-- Aşağıdaki sorgu ile oluşturulan indeksleri görebilirsiniz:
-- 
-- SELECT 
--     tablename, 
--     indexname, 
--     indexdef 
-- FROM pg_indexes 
-- WHERE tablename IN ('ref_provinces', 'ref_districts', 'ref_settlements')
-- ORDER BY tablename, indexname;
