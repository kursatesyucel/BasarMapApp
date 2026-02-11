-- Cleanup existing users before applying email verification migration
-- Run this script manually in your PostgreSQL database

-- Delete all existing users
DELETE FROM "Users";

-- Reset the sequence
ALTER SEQUENCE "Users_Id_seq" RESTART WITH 1;

-- Verify
SELECT COUNT(*) as user_count FROM "Users";
