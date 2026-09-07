-- MySqlBackup.NET 2.6.5.0
-- Dump Time: 2026-04-16 21:46:36
-- --------------------------------------
-- Server version 8.0.45 MySQL Community Server - GPL


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- 
-- Definition of __efmigrationshistory
-- 

DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE IF NOT EXISTS `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table __efmigrationshistory
-- 

/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory`(`MigrationId`,`ProductVersion`) VALUES('20260317021637_UpdateCashEntryFields','8.0.11'),('20260324130848_InitialCreate','8.0.11'),('20260325032857_AddVatToPO','8.0.11'),('20260325040724_FixAddVatToPO','8.0.11'),('20260325074122_InitDebtManagement','8.0.11');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;

-- 
-- Definition of apinvoices
-- 

DROP TABLE IF EXISTS `apinvoices`;
CREATE TABLE IF NOT EXISTS `apinvoices` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InvoiceNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `InvoiceDate` datetime(6) NOT NULL,
  `PartnerName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SubTotal` decimal(65,30) NOT NULL,
  `VatRate` decimal(65,30) NOT NULL,
  `VatAmount` decimal(65,30) NOT NULL,
  `TotalAmount` decimal(65,30) NOT NULL,
  `PaidAmount` decimal(65,30) NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table apinvoices
-- 

/*!40000 ALTER TABLE `apinvoices` DISABLE KEYS */;
/*!40000 ALTER TABLE `apinvoices` ENABLE KEYS */;

-- 
-- Definition of auditlogs
-- 

DROP TABLE IF EXISTS `auditlogs`;
CREATE TABLE IF NOT EXISTS `auditlogs` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Action` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TableName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RecordId` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `OldValues` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `NewValues` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Timestamp` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=145 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table auditlogs
-- 

/*!40000 ALTER TABLE `auditlogs` DISABLE KEYS */;
INSERT INTO `auditlogs`(`Id`,`UserName`,`Action`,`TableName`,`RecordId`,`OldValues`,`NewValues`,`Timestamp`) VALUES(1,'Hệ thống','Added','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706',NULL,'{\"Id\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.448487'),(2,'Hệ thống','Added','IdentityRole','56dfd853-5756-46fd-95a6-d8c05250a88e',NULL,'{\"Id\":\"56dfd853-5756-46fd-95a6-d8c05250a88e\",\"Name\":\"Manager\",\"NormalizedName\":\"MANAGER\"}','2026-03-24 20:13:48.569310'),(3,'Hệ thống','Added','IdentityRole','14f5762c-3558-4bf3-bac7-c7b8ba630899',NULL,'{\"Id\":\"14f5762c-3558-4bf3-bac7-c7b8ba630899\",\"Name\":\"User\",\"NormalizedName\":\"USER\"}','2026-03-24 20:13:48.583381'),(4,'Hệ thống','Added','IdentityRoleClaim`1','-2147482647',NULL,'{\"Id\":-2147482647,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Products.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.635461'),(5,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"a5d04a6c-2ba3-4df9-aaa1-990a35ae93cd\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.638396'),(6,'Hệ thống','Added','IdentityRoleClaim`1','-2147482646',NULL,'{\"Id\":-2147482646,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Products.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.657181'),(7,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"a5d04a6c-2ba3-4df9-aaa1-990a35ae93cd\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"63fc04a3-a3df-4c8a-8772-7420dd2d077a\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.657235'),(8,'Hệ thống','Added','IdentityRoleClaim`1','-2147482645',NULL,'{\"Id\":-2147482645,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Products.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.668488'),(9,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"63fc04a3-a3df-4c8a-8772-7420dd2d077a\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"49362679-80a4-4936-9585-071e08d5cc48\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.668533'),(10,'Hệ thống','Added','IdentityRoleClaim`1','-2147482644',NULL,'{\"Id\":-2147482644,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Products.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.680438'),(11,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"49362679-80a4-4936-9585-071e08d5cc48\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"1eed0e49-9625-49ed-b048-3224771cb359\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.680487'),(12,'Hệ thống','Added','IdentityRoleClaim`1','-2147482643',NULL,'{\"Id\":-2147482643,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.691320'),(13,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"1eed0e49-9625-49ed-b048-3224771cb359\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"94d782f3-aab5-42ce-8eea-335a96e096fb\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.691368'),(14,'Hệ thống','Added','IdentityRoleClaim`1','-2147482642',NULL,'{\"Id\":-2147482642,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.702974'),(15,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"94d782f3-aab5-42ce-8eea-335a96e096fb\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"3b1447b1-9ce5-4fd5-9cec-e6394c80f208\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.703024'),(16,'Hệ thống','Added','IdentityRoleClaim`1','-2147482641',NULL,'{\"Id\":-2147482641,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.713469'),(17,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"3b1447b1-9ce5-4fd5-9cec-e6394c80f208\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"4692864a-057d-4d5d-aecb-11d42d54398b\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.713530'),(18,'Hệ thống','Added','IdentityRoleClaim`1','-2147482640',NULL,'{\"Id\":-2147482640,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.724240'),(19,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"4692864a-057d-4d5d-aecb-11d42d54398b\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"96ed8abf-822d-47df-a504-0b267a7fcea1\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.724292'),(20,'Hệ thống','Added','IdentityRoleClaim`1','-2147482639',NULL,'{\"Id\":-2147482639,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.Approve\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.735796'),(21,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"96ed8abf-822d-47df-a504-0b267a7fcea1\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"600dcd27-cbf8-4316-bae3-3aabaee7ebc4\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.735847'),(22,'Hệ thống','Added','IdentityRoleClaim`1','-2147482638',NULL,'{\"Id\":-2147482638,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Documents.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.745455'),(23,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"600dcd27-cbf8-4316-bae3-3aabaee7ebc4\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"465f346a-0083-41cf-877d-518eaec4ad84\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.745499'),(24,'Hệ thống','Added','IdentityRoleClaim`1','-2147482637',NULL,'{\"Id\":-2147482637,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Documents.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.757340'),(25,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"465f346a-0083-41cf-877d-518eaec4ad84\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"06b0f622-6aa8-43c6-b046-bc5a138e16e4\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.757392'),(26,'Hệ thống','Added','IdentityRoleClaim`1','-2147482636',NULL,'{\"Id\":-2147482636,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Documents.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.768333'),(27,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"06b0f622-6aa8-43c6-b046-bc5a138e16e4\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"d7689571-2052-4556-a40e-64f18ab5f6b5\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.768383'),(28,'Hệ thống','Added','IdentityRoleClaim`1','-2147482635',NULL,'{\"Id\":-2147482635,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Warehouse.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.777591'),(29,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"d7689571-2052-4556-a40e-64f18ab5f6b5\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"d820619f-9a6c-4f1d-bd6e-ac2a3eb01ea8\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.777635'),(30,'Hệ thống','Added','IdentityRoleClaim`1','-2147482634',NULL,'{\"Id\":-2147482634,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Warehouse.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.788513'),(31,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"d820619f-9a6c-4f1d-bd6e-ac2a3eb01ea8\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"8b6f4595-028d-4dee-955c-734c16735b00\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.788554'),(32,'Hệ thống','Added','IdentityRoleClaim`1','-2147482633',NULL,'{\"Id\":-2147482633,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Warehouse.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.797620'),(33,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"8b6f4595-028d-4dee-955c-734c16735b00\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"5957e3e5-849a-48a4-a939-2da9abc5b8bb\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.797657'),(34,'Hệ thống','Added','IdentityRoleClaim`1','-2147482632',NULL,'{\"Id\":-2147482632,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Warehouse.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.807402'),(35,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"5957e3e5-849a-48a4-a939-2da9abc5b8bb\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"72176e3d-ec3c-4fa6-96f2-581b1dd365c9\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.807440'),(36,'Hệ thống','Added','IdentityRoleClaim`1','-2147482631',NULL,'{\"Id\":-2147482631,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Customers.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.819182'),(37,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"72176e3d-ec3c-4fa6-96f2-581b1dd365c9\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"a542babc-c053-4cd9-a37c-6d9ec1d24248\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.819227'),(38,'Hệ thống','Added','IdentityRoleClaim`1','-2147482630',NULL,'{\"Id\":-2147482630,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Customers.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.838951'),(39,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"a542babc-c053-4cd9-a37c-6d9ec1d24248\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"e43c6b04-607a-4d57-b322-ecb64b2de1f7\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.839011'),(40,'Hệ thống','Added','IdentityRoleClaim`1','-2147482629',NULL,'{\"Id\":-2147482629,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Customers.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.849758'),(41,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"e43c6b04-607a-4d57-b322-ecb64b2de1f7\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"130e3b5f-be58-499b-912b-944c66c93b2c\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.849808'),(42,'Hệ thống','Added','IdentityRoleClaim`1','-2147482628',NULL,'{\"Id\":-2147482628,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Customers.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.861776'),(43,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"130e3b5f-be58-499b-912b-944c66c93b2c\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"a4a39d58-12f6-46e9-a6cb-027a61061b7e\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.861822'),(44,'Hệ thống','Added','IdentityRoleClaim`1','-2147482627',NULL,'{\"Id\":-2147482627,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Users.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.874410'),(45,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"a4a39d58-12f6-46e9-a6cb-027a61061b7e\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"cc9a62a0-c3bd-4efd-ae26-45cc515790c1\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.874460'),(46,'Hệ thống','Added','IdentityRoleClaim`1','-2147482626',NULL,'{\"Id\":-2147482626,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Users.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.884482'),(47,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"cc9a62a0-c3bd-4efd-ae26-45cc515790c1\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"401338a7-0fa8-4bd6-a839-0f046daf404b\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.884521'),(48,'Hệ thống','Added','IdentityRoleClaim`1','-2147482625',NULL,'{\"Id\":-2147482625,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Users.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.896031'),(49,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"401338a7-0fa8-4bd6-a839-0f046daf404b\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"3a41f4d9-802b-49fd-beaf-4aa83e8d4247\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.896070'),(50,'Hệ thống','Added','IdentityRoleClaim`1','-2147482624',NULL,'{\"Id\":-2147482624,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Users.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.906578'),(51,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"3a41f4d9-802b-49fd-beaf-4aa83e8d4247\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"26e600c2-c40b-4ce9-8c31-f6eef0a835f2\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.906623'),(52,'Hệ thống','Added','IdentityRoleClaim`1','-2147482623',NULL,'{\"Id\":-2147482623,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Roles.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.917155'),(53,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"26e600c2-c40b-4ce9-8c31-f6eef0a835f2\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"0f712337-9a9c-42fe-a19b-c0c5c07c879c\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.917199'),(54,'Hệ thống','Added','IdentityRoleClaim`1','-2147482622',NULL,'{\"Id\":-2147482622,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Roles.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.930291'),(55,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"0f712337-9a9c-42fe-a19b-c0c5c07c879c\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"4a6e0121-58d9-4800-a841-0eea025c2c46\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.930350'),(56,'Hệ thống','Added','IdentityRoleClaim`1','-2147482621',NULL,'{\"Id\":-2147482621,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Roles.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.943896'),(57,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"4a6e0121-58d9-4800-a841-0eea025c2c46\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"04825765-5c9f-4c89-a8d7-cb530afb2222\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.943945'),(58,'Hệ thống','Added','IdentityRoleClaim`1','-2147482620',NULL,'{\"Id\":-2147482620,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Roles.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.956029'),(59,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"04825765-5c9f-4c89-a8d7-cb530afb2222\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"76951eba-e29a-4a36-881f-24d8e9340080\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.956073'),(60,'Hệ thống','Added','IdentityRoleClaim`1','-2147482619',NULL,'{\"Id\":-2147482619,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Debts.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.968361'),(61,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"76951eba-e29a-4a36-881f-24d8e9340080\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"40909969-0a2a-4e4c-ab62-20edb49953a3\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.968403'),(62,'Hệ thống','Added','IdentityRoleClaim`1','-2147482618',NULL,'{\"Id\":-2147482618,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Debts.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.980390'),(63,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"40909969-0a2a-4e4c-ab62-20edb49953a3\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"e8d84471-0c34-4487-825a-f1432efe3eb5\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.980432'),(64,'Hệ thống','Added','IdentityRoleClaim`1','-2147482617',NULL,'{\"Id\":-2147482617,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Debts.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:48.992566'),(65,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"e8d84471-0c34-4487-825a-f1432efe3eb5\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"e4b908c2-4972-4a75-9824-ae7f05921b03\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:48.992612'),(66,'Hệ thống','Added','IdentityRoleClaim`1','-2147482616',NULL,'{\"Id\":-2147482616,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Debts.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.004311'),(67,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"e4b908c2-4972-4a75-9824-ae7f05921b03\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"548695e3-855c-42bb-9da2-9fa203a93b48\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.004355'),(68,'Hệ thống','Added','IdentityRoleClaim`1','-2147482615',NULL,'{\"Id\":-2147482615,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Plans.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.016398'),(69,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"548695e3-855c-42bb-9da2-9fa203a93b48\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"03ac4bbe-9bee-45f4-a0f7-9b1c4c27e813\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.016440'),(70,'Hệ thống','Added','IdentityRoleClaim`1','-2147482614',NULL,'{\"Id\":-2147482614,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Plans.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.028620'),(71,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"03ac4bbe-9bee-45f4-a0f7-9b1c4c27e813\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"26e551a7-7d87-4af2-9ee9-b38097790c22\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.028665'),(72,'Hệ thống','Added','IdentityRoleClaim`1','-2147482613',NULL,'{\"Id\":-2147482613,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Plans.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.040799'),(73,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"26e551a7-7d87-4af2-9ee9-b38097790c22\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"b75cf6c0-fffe-4191-8271-6f40ddc3d50a\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.040837'),(74,'Hệ thống','Added','IdentityRoleClaim`1','-2147482612',NULL,'{\"Id\":-2147482612,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Plans.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.052856'),(75,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"b75cf6c0-fffe-4191-8271-6f40ddc3d50a\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"ff9f0fa1-7462-4f37-b2d4-2dee755f1776\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.052900'),(76,'Hệ thống','Added','IdentityRoleClaim`1','-2147482611',NULL,'{\"Id\":-2147482611,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Norms.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.065214'),(77,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"ff9f0fa1-7462-4f37-b2d4-2dee755f1776\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"02656c35-6c3c-4f6b-9189-ced8f4ca7769\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.065257'),(78,'Hệ thống','Added','IdentityRoleClaim`1','-2147482610',NULL,'{\"Id\":-2147482610,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Norms.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.077455'),(79,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"02656c35-6c3c-4f6b-9189-ced8f4ca7769\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"c2545e2c-f274-41d9-a517-5f56d15397d4\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.077500'),(80,'Hệ thống','Added','IdentityRoleClaim`1','-2147482609',NULL,'{\"Id\":-2147482609,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Norms.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.089648'),(81,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"c2545e2c-f274-41d9-a517-5f56d15397d4\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"e5dfd417-fe10-48c6-9288-09a10c6de38b\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.089693'),(82,'Hệ thống','Added','IdentityRoleClaim`1','-2147482608',NULL,'{\"Id\":-2147482608,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Norms.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.104518'),(83,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"e5dfd417-fe10-48c6-9288-09a10c6de38b\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"568c060f-707e-43ae-be7e-4fb4ef14843c\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.104569'),(84,'Hệ thống','Added','IdentityRoleClaim`1','-2147482607',NULL,'{\"Id\":-2147482607,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Shipments.View\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.117122'),(85,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"568c060f-707e-43ae-be7e-4fb4ef14843c\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"2ae64d77-59cf-449a-ac99-f04c07f95839\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.117166'),(86,'Hệ thống','Added','IdentityRoleClaim`1','-2147482606',NULL,'{\"Id\":-2147482606,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Shipments.Create\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.128293'),(87,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"2ae64d77-59cf-449a-ac99-f04c07f95839\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"a5191aab-927e-4ac5-92a9-0962909faff1\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.128329'),(88,'Hệ thống','Added','IdentityRoleClaim`1','-2147482605',NULL,'{\"Id\":-2147482605,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Shipments.Edit\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.139887'),(89,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"a5191aab-927e-4ac5-92a9-0962909faff1\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"7c59f926-07b0-4691-b59c-5b9cf77f7c44\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.139926'),(90,'Hệ thống','Added','IdentityRoleClaim`1','-2147482604',NULL,'{\"Id\":-2147482604,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Shipments.Delete\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.151442'),(91,'Hệ thống','Modified','IdentityRole','bdb4b9e1-809e-4fe8-b86e-897227fac706','{\"ConcurrencyStamp\":\"7c59f926-07b0-4691-b59c-5b9cf77f7c44\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"d0b20ed2-79be-4e98-87a0-bcf2c863d446\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-03-24 20:13:49.151522'),(92,'Hệ thống','Added','AppUser','90546b19-e872-48e9-abea-9021c20c4fe2',NULL,'{\"Id\":\"90546b19-e872-48e9-abea-9021c20c4fe2\",\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"e93633c8-fa1d-4149-a971-16d1fa6a5334\",\"CreatedAt\":\"2026-03-24T20:13:49.1696946+07:00\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAELmGbHakem76AjgzNObbYHzSS7Nj/0eO32VA3GUQQ6Xl0nB9GoxWObaSGP\\u002Bf3yCiEA==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"554UOO6SBX3ZH5BDS2JBZS3O6RHM2D4M\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-03-24 20:13:49.266244'),(93,'Hệ thống','Added','IdentityUserRole`1','bdb4b9e1-809e-4fe8-b86e-897227fac706',NULL,'{\"UserId\":\"90546b19-e872-48e9-abea-9021c20c4fe2\",\"RoleId\":\"bdb4b9e1-809e-4fe8-b86e-897227fac706\"}','2026-03-24 20:13:49.320000'),(94,'Hệ thống','Modified','AppUser','90546b19-e872-48e9-abea-9021c20c4fe2','{\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"e93633c8-fa1d-4149-a971-16d1fa6a5334\",\"CreatedAt\":\"2026-03-24T20:13:49.1696946+07:00\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAELmGbHakem76AjgzNObbYHzSS7Nj/0eO32VA3GUQQ6Xl0nB9GoxWObaSGP\\u002Bf3yCiEA==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"554UOO6SBX3ZH5BDS2JBZS3O6RHM2D4M\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','{\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"59607e75-4dc8-4053-a38d-6071f52467a0\",\"CreatedAt\":\"2026-03-24T20:13:49.1696946+07:00\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAELmGbHakem76AjgzNObbYHzSS7Nj/0eO32VA3GUQQ6Xl0nB9GoxWObaSGP\\u002Bf3yCiEA==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"554UOO6SBX3ZH5BDS2JBZS3O6RHM2D4M\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-03-24 20:13:49.320035'),(95,'Hệ thống','Modified','AppUser','90546b19-e872-48e9-abea-9021c20c4fe2','{\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"59607e75-4dc8-4053-a38d-6071f52467a0\",\"CreatedAt\":\"2026-03-24T20:13:49.169694\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAELmGbHakem76AjgzNObbYHzSS7Nj/0eO32VA3GUQQ6Xl0nB9GoxWObaSGP\\u002Bf3yCiEA==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"554UOO6SBX3ZH5BDS2JBZS3O6RHM2D4M\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','{\"AccessFailedCount\":1,\"Address\":\"\",\"ConcurrencyStamp\":\"2e8b4d73-7177-422c-9972-e8a7abfc5662\",\"CreatedAt\":\"2026-03-24T20:13:49.169694\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAELmGbHakem76AjgzNObbYHzSS7Nj/0eO32VA3GUQQ6Xl0nB9GoxWObaSGP\\u002Bf3yCiEA==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"554UOO6SBX3ZH5BDS2JBZS3O6RHM2D4M\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-03-24 20:14:46.503583'),(96,'Hệ thống','Modified','AppUser','90546b19-e872-48e9-abea-9021c20c4fe2','{\"AccessFailedCount\":1,\"Address\":\"\",\"ConcurrencyStamp\":\"2e8b4d73-7177-422c-9972-e8a7abfc5662\",\"CreatedAt\":\"2026-03-24T20:13:49.169694\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAELmGbHakem76AjgzNObbYHzSS7Nj/0eO32VA3GUQQ6Xl0nB9GoxWObaSGP\\u002Bf3yCiEA==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"554UOO6SBX3ZH5BDS2JBZS3O6RHM2D4M\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','{\"AccessFailedCount\":2,\"Address\":\"\",\"ConcurrencyStamp\":\"ff081f3d-e3ce-455e-b4c3-984e0a7d4aea\",\"CreatedAt\":\"2026-03-24T20:13:49.169694\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAELmGbHakem76AjgzNObbYHzSS7Nj/0eO32VA3GUQQ6Xl0nB9GoxWObaSGP\\u002Bf3yCiEA==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"554UOO6SBX3ZH5BDS2JBZS3O6RHM2D4M\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-03-24 20:14:56.150933'),(97,'Hệ thống','Modified','AppUser','90546b19-e872-48e9-abea-9021c20c4fe2','{\"AccessFailedCount\":2,\"Address\":\"\",\"ConcurrencyStamp\":\"ff081f3d-e3ce-455e-b4c3-984e0a7d4aea\",\"CreatedAt\":\"2026-03-24T20:13:49.169694\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAELmGbHakem76AjgzNObbYHzSS7Nj/0eO32VA3GUQQ6Xl0nB9GoxWObaSGP\\u002Bf3yCiEA==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"554UOO6SBX3ZH5BDS2JBZS3O6RHM2D4M\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','{\"AccessFailedCount\":2,\"Address\":\"\",\"ConcurrencyStamp\":\"e05b687a-eebd-4b9f-a954-c401771c03b5\",\"CreatedAt\":\"2026-03-24T20:13:49.169694\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAED9d4aPm0s8ObC6SOZRIskkzm5Vh6wZm8lDc3KK/VWe5pSead/0gxpyIWblcYCF94g==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"5XUBQWYCNVMZMV6GM2F5CDUKYZ6Z4GDN\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-03-24 20:15:52.531429'),(98,'Hệ thống','Modified','AppUser','90546b19-e872-48e9-abea-9021c20c4fe2','{\"AccessFailedCount\":2,\"Address\":\"\",\"ConcurrencyStamp\":\"e05b687a-eebd-4b9f-a954-c401771c03b5\",\"CreatedAt\":\"2026-03-24T20:13:49.169694\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAED9d4aPm0s8ObC6SOZRIskkzm5Vh6wZm8lDc3KK/VWe5pSead/0gxpyIWblcYCF94g==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"5XUBQWYCNVMZMV6GM2F5CDUKYZ6Z4GDN\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','{\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"943515bd-89bd-48f1-9e2f-9f149f4cbb62\",\"CreatedAt\":\"2026-03-24T20:13:49.169694\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAED9d4aPm0s8ObC6SOZRIskkzm5Vh6wZm8lDc3KK/VWe5pSead/0gxpyIWblcYCF94g==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"5XUBQWYCNVMZMV6GM2F5CDUKYZ6Z4GDN\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-03-24 20:16:01.934248'),(99,'sitinhtutu6@gmail.com','Added','Customer','-2147482647',NULL,'{\"Id\":-2147482647,\"CompanyName\":\"C\\u00D4NG TY TNHH ABC\",\"CustomerCode\":\"KH2603-1408\",\"Type\":3}','2026-03-24 20:16:18.569250'),(100,'sitinhtutu6@gmail.com','Added','WarehouseItem','-2147482647',NULL,'{\"Id\":-2147482647,\"CategoryId\":4,\"Code\":\"VT-0001\",\"CostPrice\":0,\"ItemType\":\"Material\",\"Name\":\"G\\u1ED7 ASH\",\"StockQuantity\":0,\"Unit\":\"C\\u00E1i\"}','2026-03-24 20:16:42.376488'),(101,'sitinhtutu6@gmail.com','Modified','Customer','1','{\"CompanyName\":\"C\\u00D4NG TY TNHH ABC\",\"CustomerCode\":\"KH2603-1408\",\"Type\":3}','{\"CompanyName\":\"C\\u00D4NG TY TNHH ABC\",\"CustomerCode\":\"KH2603-1408\",\"Type\":3}','2026-03-24 20:16:57.086752'),(102,'sitinhtutu6@gmail.com','Modified','Customer','1','{\"CompanyName\":\"C\\u00D4NG TY TNHH ABC\",\"CustomerCode\":\"KH2603-1408\",\"Type\":2}','{\"CompanyName\":\"C\\u00D4NG TY TNHH ABC\",\"CustomerCode\":\"KH2603-1408\",\"Type\":2}','2026-03-24 20:17:06.763971'),(103,'sitinhtutu6@gmail.com','Added','PurchaseOrder','-2147482647',NULL,'{\"Id\":-2147482647,\"DepositAmount\":550000,\"Note\":\"\",\"OrderDate\":\"2026-03-24T00:00:00\",\"POCode\":\"PO-260324-417\",\"PaymentStatusPO\":1,\"RemainingAmount\":-550000,\"Status\":0,\"SupplierId\":1,\"SupplierName\":\"C\\u00D4NG TY TNHH ABC\",\"TotalAmount\":0,\"Type\":2}','2026-03-24 20:32:04.143052'),(104,'sitinhtutu6@gmail.com','Added','PurchaseOrderDetail','-2147482647',NULL,'{\"Id\":-2147482647,\"IsFinished\":false,\"PurchaseOrderId\":-2147482647,\"QuantityBad\":0,\"QuantityOrdered\":10,\"QuantityReceived\":0,\"UnitPrice\":0,\"WarehouseItemId\":1}','2026-03-24 20:32:04.171366'),(105,'sitinhtutu6@gmail.com','Added','StockTransaction','-2147482647',NULL,'{\"Id\":-2147482647,\"CurrentStock\":10,\"DocumentCode\":\"PN-20260325-1920\",\"Note\":\"\",\"Price\":0,\"Quantity\":10,\"Receiver\":\"C\\u00D4NG TY TNHH ABC\",\"Staff\":\"sitinhtutu6@gmail.com\",\"TransactionDate\":\"2026-03-25T12:20:27.621Z\",\"Type\":\"Import\",\"WarehouseItemId\":1}','2026-03-25 19:20:28.002081'),(106,'sitinhtutu6@gmail.com','Added','MaterialImport','-2147482647',NULL,'{\"Id\":-2147482647,\"ImportCode\":\"PN-20260325-1920\",\"ImportDate\":\"2026-03-25T12:20:27.621Z\",\"IsLocked\":false,\"PaidAmount\":0,\"PaymentStatus\":2,\"SubTotal\":0,\"SupplierId\":0,\"SupplierName\":\"C\\u00D4NG TY TNHH ABC\",\"TotalAmount\":0,\"VatAmount\":0,\"VatRate\":0}','2026-03-25 19:20:28.028744'),(107,'sitinhtutu6@gmail.com','Modified','WarehouseItem','1','{\"StockQuantity\":0}','{\"StockQuantity\":10}','2026-03-25 19:20:28.038492'),(108,'sitinhtutu6@gmail.com','Modified','PurchaseOrder','1','{\"Status\":0}','{\"Status\":2}','2026-03-25 19:20:28.301149'),(109,'sitinhtutu6@gmail.com','Modified','PurchaseOrderDetail','1','{\"IsFinished\":false,\"QuantityReceived\":0}','{\"IsFinished\":true,\"QuantityReceived\":10}','2026-03-25 19:20:28.304624'),(110,'sitinhtutu6@gmail.com','Modified','PurchaseOrder','1','{\"IsDebtFinalized\":false}','{\"DebtFinalizedDate\":\"2026-03-25T20:47:05.9912427+07:00\",\"InvoiceNumber\":\"HD-005\",\"IsDebtFinalized\":true}','2026-03-25 20:47:06.057283'),(111,'sitinhtutu6@gmail.com','Added','WarehouseItem','-2147482647',NULL,'{\"Id\":-2147482647,\"CategoryId\":3,\"Code\":\"SP-0001\",\"CostPrice\":10000,\"ItemType\":\"Product\",\"Name\":\"Gh\\u1EBF KT3\",\"Note\":\"\\u0110\\u01B0\\u1EE3c t\\u1EA1o t\\u1EF1 \\u0111\\u1ED9ng t\\u1EEB Module S\\u1EA3n Ph\\u1EA9m\",\"StockQuantity\":0,\"Unit\":\"C\\u00E1i\"}','2026-03-26 21:31:31.882982'),(112,'sitinhtutu6@gmail.com','Added','Product','-2147482647',NULL,'{\"Id\":-2147482647,\"Color\":\"Xanh\",\"DefaultPrice\":10000,\"HasVariants\":true,\"Length\":1,\"MaterialType\":\"Beech\",\"ProductCode\":\"SP-0001\",\"ProductName\":\"Gh\\u1EBF KT3\",\"Thick\":1,\"Unit\":\"C\\u00E1i\",\"WarehouseItemId\":2,\"Width\":1}','2026-03-26 21:31:32.091017'),(113,'sitinhtutu6@gmail.com','Modified','WarehouseItem','2','{\"CategoryId\":3,\"Code\":\"SP-0001\",\"CostPrice\":10000,\"ItemType\":\"Product\",\"Name\":\"Gh\\u1EBF KT3\",\"Note\":\"\\u0110\\u01B0\\u1EE3c t\\u1EA1o t\\u1EF1 \\u0111\\u1ED9ng t\\u1EEB Module S\\u1EA3n Ph\\u1EA9m\",\"StockQuantity\":0,\"Unit\":\"C\\u00E1i\"}','{\"CategoryId\":3,\"Code\":\"SP-0001\",\"CostPrice\":10000,\"ItemType\":\"Product\",\"LinkedProductId\":1,\"Name\":\"Gh\\u1EBF KT3\",\"Note\":\"\\u0110\\u01B0\\u1EE3c t\\u1EA1o t\\u1EF1 \\u0111\\u1ED9ng t\\u1EEB Module S\\u1EA3n Ph\\u1EA9m\",\"StockQuantity\":0,\"Unit\":\"C\\u00E1i\"}','2026-03-26 21:31:32.103562'),(114,'sitinhtutu6@gmail.com','Added','Order','-2147482647',NULL,'{\"Id\":-2147482647,\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.2478648+07:00\",\"PaidAmount\":0,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":0,\"TotalAmount\":1080000.00,\"TotalCOGS\":0}','2026-03-26 21:32:10.345557'),(115,'sitinhtutu6@gmail.com','Added','OrderDetail','-2147482647',NULL,'{\"Id\":-2147482647,\"OrderId\":-2147482647,\"ProductId\":1,\"ProductName\":\"Gh\\u1EBF KT3\",\"Quantity\":100,\"Specifications\":\"1x1x1\",\"UnitPrice\":10000,\"VatPercent\":8}','2026-03-26 21:32:10.360920'),(116,'sitinhtutu6@gmail.com','Added','ExportShipment','-2147482647',NULL,'{\"Id\":-2147482647,\"CreatedDate\":\"2026-03-26T21:32:42.57313+07:00\",\"CurrentStep\":1,\"CustomerId\":1,\"ETD\":\"2026-03-26T00:00:00\",\"LicensePlate\":\"51G-531.89\",\"PaymentStatus\":0,\"ProgressPercent\":0,\"ShipmentCode\":\"TRX-2603-B3E2\",\"ShippingCost\":0,\"Status\":0,\"SubTotal\":0,\"TransportType\":1,\"VatAmount\":0,\"VatRate\":0}','2026-03-26 21:32:42.632875'),(117,'sitinhtutu6@gmail.com','Modified','Order','1','{\"Status\":0}','{\"Status\":1}','2026-03-26 21:33:13.950870'),(118,'sitinhtutu6@gmail.com','Modified','Order','1','{\"Status\":1}','{\"Status\":3}','2026-03-26 21:33:18.084059'),(119,'sitinhtutu6@gmail.com','Modified','Order','1','{\"Status\":3}','{\"Status\":4}','2026-03-26 21:33:22.800104'),(120,'sitinhtutu6@gmail.com','Modified','Order','1','{\"Status\":4}','{\"Status\":5}','2026-03-26 21:33:26.585408'),(121,'sitinhtutu6@gmail.com','Modified','Order','1','{\"Status\":5}','{\"Status\":6}','2026-03-26 21:33:31.646376'),(122,'sitinhtutu6@gmail.com','Modified','Order','1','{\"Status\":6}','{\"Status\":7}','2026-03-26 21:33:35.915857'),(123,'sitinhtutu6@gmail.com','Modified','Order','1','{\"Status\":7}','{\"Status\":2}','2026-03-26 21:33:39.635688'),(124,'sitinhtutu6@gmail.com','Added','Shipment','-2147482647',NULL,'{\"Id\":-2147482647,\"CustomerId\":1,\"ExportShipmentId\":1,\"OrderId\":1,\"ShipmentCode\":\"PX-20260326-5345\",\"ShipmentDate\":\"2026-03-26T00:00:00\",\"Type\":0,\"VehicleNumber\":\"51G-531.89\"}','2026-03-26 21:34:18.182708'),(125,'sitinhtutu6@gmail.com','Added','ShipmentDetail','-2147482647',NULL,'{\"Id\":-2147482647,\"OrderDetailId\":1,\"QuantityShipped\":50,\"ShipmentId\":-2147482647}','2026-03-26 21:34:18.183427'),(126,'sitinhtutu6@gmail.com','Modified','ExportShipment','1','{\"Status\":0}','{\"Status\":1}','2026-03-26 21:34:18.183457'),(127,'sitinhtutu6@gmail.com','Modified','Order','1','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.247864\",\"PaidAmount\":0.00,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":2,\"TotalAmount\":1080000.00,\"TotalCOGS\":0.00}','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.247864\",\"PaidAmount\":0.00,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":5,\"TotalAmount\":1080000.00,\"TotalCOGS\":0.00}','2026-03-26 21:34:18.220320'),(128,'sitinhtutu6@gmail.com','Added','ExportShipment','-2147482646',NULL,'{\"Id\":-2147482646,\"CreatedDate\":\"2026-03-26T21:34:34.4035448+07:00\",\"CurrentStep\":1,\"CustomerId\":1,\"ETD\":\"2026-03-26T00:00:00\",\"PaymentStatus\":0,\"ProgressPercent\":0,\"ShipmentCode\":\"TRX-2603-A0A3\",\"ShippingCost\":0,\"Status\":0,\"SubTotal\":0,\"TransportType\":1,\"VatAmount\":0,\"VatRate\":0}','2026-03-26 21:34:34.403778'),(129,'sitinhtutu6@gmail.com','Added','Invoice','-2147482647',NULL,'{\"Id\":-2147482647,\"CreatedAt\":\"2026-03-27T22:12:32.7097067+07:00\",\"InvoiceDate\":\"2026-03-27T00:00:00\",\"InvoiceNumber\":\"HD-001\",\"TotalAmount\":580000}','2026-03-27 22:12:32.767302'),(130,'sitinhtutu6@gmail.com','Added','InvoiceDetail','-2147482647',NULL,'{\"Id\":-2147482647,\"BilledAmount\":580000,\"InvoiceId\":1,\"OrderId\":1}','2026-03-27 22:12:32.938525'),(131,'sitinhtutu6@gmail.com','Modified','Order','1','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.247864\",\"PaidAmount\":0.00,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":5,\"TotalAmount\":1080000.00,\"TotalCOGS\":0.00}','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.247864\",\"PaidAmount\":0.00,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":6,\"TotalAmount\":1080000.00,\"TotalCOGS\":0.00}','2026-03-27 22:12:32.938612'),(132,'sitinhtutu6@gmail.com','Added','CashEntry','-2147482647',NULL,'{\"Id\":-2147482647,\"AllocatedMonths\":1,\"Amount\":380000,\"Category\":1,\"CreatedAt\":\"2026-04-04T15:17:31.9097916+07:00\",\"CreatedBy\":\"sitinhtutu6@gmail.com\",\"Description\":\"Thu ti\\u1EC1n h\\u00E0ng\",\"Method\":2,\"OrderId\":1,\"PaperVoucherNumber\":\"PT-001\",\"ReportDate\":\"2026-04-04T00:00:00\",\"TargetName\":\"C\\u00D4NG TY TNHH ABC\",\"TransactionDate\":\"2026-04-04T00:00:00\",\"Type\":1,\"VoucherCode\":\"PT-2604-9793\"}','2026-04-04 15:17:32.044383'),(133,'sitinhtutu6@gmail.com','Modified','Order','1','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.247864\",\"PaidAmount\":0.00,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":6,\"TotalAmount\":1080000.00,\"TotalCOGS\":0.00}','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.247864\",\"PaidAmount\":0.00,\"PaymentStatus\":1,\"ProductionIndex\":9999,\"Status\":6,\"TotalAmount\":1080000.00,\"TotalCOGS\":0.00}','2026-04-04 15:17:32.079636'),(134,'sitinhtutu6@gmail.com','Deleted','CashEntry','1','{\"Id\":1,\"AllocatedMonths\":1,\"Amount\":380000.00,\"Category\":1,\"CreatedAt\":\"2026-04-04T15:17:31.909791\",\"CreatedBy\":\"sitinhtutu6@gmail.com\",\"Description\":\"Thu ti\\u1EC1n h\\u00E0ng\",\"Method\":2,\"OrderId\":1,\"PaperVoucherNumber\":\"PT-001\",\"ReportDate\":\"2026-04-04T00:00:00\",\"TargetName\":\"C\\u00D4NG TY TNHH ABC\",\"TransactionDate\":\"2026-04-04T00:00:00\",\"Type\":1,\"VoucherCode\":\"PT-2604-9793\"}',NULL,'2026-04-04 15:17:53.358261'),(135,'sitinhtutu6@gmail.com','Modified','Order','1','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.247864\",\"PaidAmount\":0.00,\"PaymentStatus\":1,\"ProductionIndex\":9999,\"Status\":6,\"TotalAmount\":1080000.00,\"TotalCOGS\":0.00}','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-03-26T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-001\",\"OrderDate\":\"2026-03-26T21:32:10.247864\",\"PaidAmount\":0.00,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":6,\"TotalAmount\":1080000.00,\"TotalCOGS\":0.00}','2026-04-04 15:17:53.392912'),(136,'sitinhtutu6@gmail.com','Added','CashEntry','-2147482647',NULL,'{\"Id\":-2147482647,\"AllocatedMonths\":1,\"Amount\":250000,\"Category\":1,\"CreatedAt\":\"2026-04-08T16:17:35.6235031+07:00\",\"CreatedBy\":\"sitinhtutu6@gmail.com\",\"Method\":1,\"PaperVoucherNumber\":\"PT-001\",\"ReportDate\":\"2026-04-08T16:17:35.6001257+07:00\",\"TargetName\":\"H\\u1EA3o\",\"TransactionDate\":\"2026-04-08T00:00:00\",\"Type\":1,\"VoucherCode\":\"PT2604-001\"}','2026-04-08 16:17:35.682593'),(137,'sitinhtutu6@gmail.com','Added','Order','-2147482647',NULL,'{\"Id\":-2147482647,\"CustomerId\":1,\"DeliveryDeadline\":\"2026-04-20T00:00:00\",\"DepositAmount\":0,\"IsLocked\":false,\"OrderCode\":\"DH-002\",\"OrderDate\":\"2026-04-15T21:52:09.2310446+07:00\",\"PaidAmount\":0,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":0,\"TotalAmount\":16200000.00,\"TotalCOGS\":0}','2026-04-15 21:52:09.372933'),(138,'sitinhtutu6@gmail.com','Added','OrderDetail','-2147482647',NULL,'{\"Id\":-2147482647,\"OrderId\":-2147482647,\"ProductId\":1,\"ProductName\":\"Gh\\u1EBF KT3\",\"Quantity\":100,\"Specifications\":\"1x1x1\",\"UnitPrice\":150000,\"VatPercent\":8}','2026-04-15 21:52:09.397602'),(139,'sitinhtutu6@gmail.com','Modified','Order','2','{\"Status\":0}','{\"Status\":1}','2026-04-15 21:52:53.337742'),(140,'sitinhtutu6@gmail.com','Modified','Order','2','{\"Status\":1}','{\"Status\":0}','2026-04-15 21:53:18.256034'),(141,'sitinhtutu6@gmail.com','Modified','Order','2','{\"Status\":0}','{\"Status\":1}','2026-04-15 21:53:33.600533'),(142,'sitinhtutu6@gmail.com','Modified','Order','2','{\"Status\":1}','{\"Status\":0}','2026-04-15 21:58:34.551932'),(143,'sitinhtutu6@gmail.com','Modified','Order','2','{\"Status\":0}','{\"Status\":2}','2026-04-15 21:58:41.595752'),(144,'sitinhtutu6@gmail.com','Modified','Order','2','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-04-20T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-002\",\"OrderDate\":\"2026-04-15T21:52:09.231044\",\"PaidAmount\":0.00,\"PaymentStatus\":0,\"ProductionIndex\":9999,\"Status\":2,\"TotalAmount\":16200000.00,\"TotalCOGS\":0.00}','{\"CustomerId\":1,\"DeliveryDeadline\":\"2026-04-20T00:00:00\",\"DepositAmount\":0.00,\"IsLocked\":false,\"OrderCode\":\"DH-002\",\"OrderDate\":\"2026-04-15T21:52:09.231044\",\"PaidAmount\":0.00,\"PaymentStatus\":0,\"ProductionIndex\":1,\"Status\":2,\"TotalAmount\":16200000.00,\"TotalCOGS\":0.00}','2026-04-15 21:59:19.129696');
/*!40000 ALTER TABLE `auditlogs` ENABLE KEYS */;

-- 
-- Definition of companyconfigs
-- 

DROP TABLE IF EXISTS `companyconfigs`;
CREATE TABLE IF NOT EXISTS `companyconfigs` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CompanyName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `TaxCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Phone` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table companyconfigs
-- 

/*!40000 ALTER TABLE `companyconfigs` DISABLE KEYS */;
/*!40000 ALTER TABLE `companyconfigs` ENABLE KEYS */;

-- 
-- Definition of costcategories
-- 

DROP TABLE IF EXISTS `costcategories`;
CREATE TABLE IF NOT EXISTS `costcategories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Section` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table costcategories
-- 

/*!40000 ALTER TABLE `costcategories` DISABLE KEYS */;
INSERT INTO `costcategories`(`Id`,`Name`,`Section`,`IsActive`,`Description`) VALUES(1,'Lương cơ bản nhân viên',1,1,'Chi trả lương cứng hàng tháng'),(2,'Tiền tăng ca, phụ cấp, thưởng',1,1,'Chi phí làm thêm giờ, phụ cấp'),(3,'Bảo hiểm xã hội, BHYT',1,1,'Phần BH công ty đóng'),(4,'Tiền điện, nước, rác',2,1,'Chi phí điện nước tại xưởng/VP'),(5,'Tiền thuê mặt bằng, nhà xưởng',2,1,'Chi phí thuê địa điểm'),(6,'Bảo trì, sửa chữa, khấu hao',2,1,'Khấu hao TSCĐ, sửa chữa'),(7,'Tiền xăng xe, cầu đường',3,1,'Chi phí xe công ty'),(8,'Phí thuê xe ngoài, bưu cục',3,1,'ViettelPost, thuê tải ngoài'),(9,'Vật tư bao bì, đóng gói',3,1,'Băng keo, thùng carton'),(10,'Văn phòng phẩm',4,1,'Giấy, mực, bút...'),(11,'Cước Internet, Phần mềm',4,1,'Cước mạng, phần mềm ERP'),(12,'Chi phí tiếp khách',4,1,'Ngoại giao, ăn uống'),(13,'Quảng cáo, Truyền thông',5,1,'Ads Facebook, Google'),(14,'Chiết khấu, Khuyến mãi',5,1,'Quà tặng, hoa hồng'),(15,'Lệ phí ngân hàng, Lãi vay',99,1,'Phí chuyển khoản, lãi vay'),(16,'Thuế, Lệ phí nhà nước',99,1,'Thuế môn bài, phí môi trường');
/*!40000 ALTER TABLE `costcategories` ENABLE KEYS */;

-- 
-- Definition of customers
-- 

DROP TABLE IF EXISTS `customers`;
CREATE TABLE IF NOT EXISTS `customers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CustomerCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CompanyName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` int NOT NULL,
  `TaxCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `BankAccount` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `BankName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ContactPerson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Notes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table customers
-- 

/*!40000 ALTER TABLE `customers` DISABLE KEYS */;
INSERT INTO `customers`(`Id`,`CustomerCode`,`CompanyName`,`Type`,`TaxCode`,`BankAccount`,`BankName`,`ContactPerson`,`PhoneNumber`,`Email`,`Address`,`Notes`) VALUES(1,'KH2603-1408','CÔNG TY TNHH ABC',2,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);
/*!40000 ALTER TABLE `customers` ENABLE KEYS */;

-- 
-- Definition of documents
-- 

DROP TABLE IF EXISTS `documents`;
CREATE TABLE IF NOT EXISTS `documents` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `FileName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SystemFileName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FilePath` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Group` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FileSize` bigint NOT NULL,
  `UploadedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table documents
-- 

/*!40000 ALTER TABLE `documents` DISABLE KEYS */;
/*!40000 ALTER TABLE `documents` ENABLE KEYS */;

-- 
-- Definition of exportshipments
-- 

DROP TABLE IF EXISTS `exportshipments`;
CREATE TABLE IF NOT EXISTS `exportshipments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ShipmentCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CustomerId` int NOT NULL,
  `CreatedDate` datetime(6) NOT NULL,
  `ETD` datetime(6) DEFAULT NULL,
  `ETA` datetime(6) DEFAULT NULL,
  `BookingNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `VesselName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ContainerNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `SealNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `BLNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CurrentStep` int NOT NULL,
  `ProgressPercent` int NOT NULL,
  `TransportType` int NOT NULL,
  `CarrierName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `LicensePlate` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ShippingCost` decimal(18,2) NOT NULL,
  `PaymentStatus` int NOT NULL,
  `Status` int NOT NULL,
  `SubTotal` decimal(18,2) NOT NULL,
  `VatRate` double NOT NULL,
  `VatAmount` decimal(18,2) NOT NULL,
  `PriceHistoryJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `APInvoiceId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ExportShipments_APInvoiceId` (`APInvoiceId`),
  KEY `IX_ExportShipments_CustomerId` (`CustomerId`),
  CONSTRAINT `FK_ExportShipments_APInvoices_APInvoiceId` FOREIGN KEY (`APInvoiceId`) REFERENCES `apinvoices` (`Id`),
  CONSTRAINT `FK_ExportShipments_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table exportshipments
-- 

/*!40000 ALTER TABLE `exportshipments` DISABLE KEYS */;
INSERT INTO `exportshipments`(`Id`,`ShipmentCode`,`CustomerId`,`CreatedDate`,`ETD`,`ETA`,`BookingNumber`,`VesselName`,`ContainerNumber`,`SealNumber`,`BLNumber`,`CurrentStep`,`ProgressPercent`,`TransportType`,`CarrierName`,`LicensePlate`,`ShippingCost`,`PaymentStatus`,`Status`,`SubTotal`,`VatRate`,`VatAmount`,`PriceHistoryJson`,`APInvoiceId`) VALUES(1,'TRX-2603-B3E2',1,'2026-03-26 21:32:42.573130','2026-03-26 00:00:00.000000',NULL,NULL,NULL,NULL,NULL,NULL,1,0,1,NULL,'51G-531.89',0.00,0,1,0.00,0,0.00,NULL,NULL),(2,'TRX-2603-A0A3',1,'2026-03-26 21:34:34.403544','2026-03-26 00:00:00.000000',NULL,NULL,NULL,NULL,NULL,NULL,1,0,1,NULL,NULL,0.00,0,0,0.00,0,0.00,NULL,NULL);
/*!40000 ALTER TABLE `exportshipments` ENABLE KEYS */;

-- 
-- Definition of exportdocuments
-- 

DROP TABLE IF EXISTS `exportdocuments`;
CREATE TABLE IF NOT EXISTS `exportdocuments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ExportShipmentId` int NOT NULL,
  `Step` int NOT NULL,
  `FileName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FilePath` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UploadDate` datetime(6) NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_ExportDocuments_ExportShipmentId` (`ExportShipmentId`),
  CONSTRAINT `FK_ExportDocuments_ExportShipments_ExportShipmentId` FOREIGN KEY (`ExportShipmentId`) REFERENCES `exportshipments` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table exportdocuments
-- 

/*!40000 ALTER TABLE `exportdocuments` DISABLE KEYS */;
/*!40000 ALTER TABLE `exportdocuments` ENABLE KEYS */;

-- 
-- Definition of financialperiods
-- 

DROP TABLE IF EXISTS `financialperiods`;
CREATE TABLE IF NOT EXISTS `financialperiods` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Month` int NOT NULL,
  `Year` int NOT NULL,
  `IsClosed` tinyint(1) NOT NULL,
  `ClosedAt` datetime(6) DEFAULT NULL,
  `ClosedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table financialperiods
-- 

/*!40000 ALTER TABLE `financialperiods` DISABLE KEYS */;
/*!40000 ALTER TABLE `financialperiods` ENABLE KEYS */;

-- 
-- Definition of invoices
-- 

DROP TABLE IF EXISTS `invoices`;
CREATE TABLE IF NOT EXISTS `invoices` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InvoiceNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `InvoiceDate` datetime(6) NOT NULL,
  `InvoiceFile` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `TotalAmount` decimal(65,30) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `RowVersion` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table invoices
-- 

/*!40000 ALTER TABLE `invoices` DISABLE KEYS */;
INSERT INTO `invoices`(`Id`,`InvoiceNumber`,`InvoiceDate`,`InvoiceFile`,`TotalAmount`,`CreatedAt`,`RowVersion`) VALUES(1,'HD-001','2026-03-27 00:00:00.000000',NULL,580000.00000000000000000000000,'2026-03-27 22:12:32.709706','2026-04-04 05:10:16.776470');
/*!40000 ALTER TABLE `invoices` ENABLE KEYS */;

-- 
-- Definition of materialimports
-- 

DROP TABLE IF EXISTS `materialimports`;
CREATE TABLE IF NOT EXISTS `materialimports` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ImportCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ImportDate` datetime(6) NOT NULL,
  `SupplierId` int NOT NULL,
  `SupplierName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TotalAmount` decimal(65,30) NOT NULL,
  `PaidAmount` decimal(65,30) NOT NULL,
  `PaymentStatus` int NOT NULL,
  `IsLocked` tinyint(1) NOT NULL,
  `SubTotal` decimal(18,2) NOT NULL,
  `VatRate` double NOT NULL,
  `VatAmount` decimal(18,2) NOT NULL,
  `APInvoiceId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MaterialImports_APInvoiceId` (`APInvoiceId`),
  CONSTRAINT `FK_MaterialImports_APInvoices_APInvoiceId` FOREIGN KEY (`APInvoiceId`) REFERENCES `apinvoices` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table materialimports
-- 

/*!40000 ALTER TABLE `materialimports` DISABLE KEYS */;
INSERT INTO `materialimports`(`Id`,`ImportCode`,`ImportDate`,`SupplierId`,`SupplierName`,`TotalAmount`,`PaidAmount`,`PaymentStatus`,`IsLocked`,`SubTotal`,`VatRate`,`VatAmount`,`APInvoiceId`) VALUES(1,'PN-20260325-1920','2026-03-25 12:20:27.621000',0,'CÔNG TY TNHH ABC',0.0000000000000000000000000000,0.0000000000000000000000000000,2,0,0.00,0,0.00,NULL);
/*!40000 ALTER TABLE `materialimports` ENABLE KEYS */;

-- 
-- Definition of importpricehistories
-- 

DROP TABLE IF EXISTS `importpricehistories`;
CREATE TABLE IF NOT EXISTS `importpricehistories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MaterialImportId` int NOT NULL,
  `OldTotalAmount` decimal(65,30) NOT NULL,
  `NewTotalAmount` decimal(65,30) NOT NULL,
  `Reason` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ChangedAt` datetime(6) NOT NULL,
  `ChangedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ImportPriceHistories_MaterialImportId` (`MaterialImportId`),
  CONSTRAINT `FK_ImportPriceHistories_MaterialImports_MaterialImportId` FOREIGN KEY (`MaterialImportId`) REFERENCES `materialimports` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table importpricehistories
-- 

/*!40000 ALTER TABLE `importpricehistories` DISABLE KEYS */;
/*!40000 ALTER TABLE `importpricehistories` ENABLE KEYS */;

-- 
-- Definition of orderhistory
-- 

DROP TABLE IF EXISTS `orderhistory`;
CREATE TABLE IF NOT EXISTS `orderhistory` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderId` int DEFAULT NULL,
  `Action` varchar(255) NOT NULL,
  `OldValue` text,
  `NewValue` text,
  `ChangedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `ChangedBy` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table orderhistory
-- 

/*!40000 ALTER TABLE `orderhistory` DISABLE KEYS */;
/*!40000 ALTER TABLE `orderhistory` ENABLE KEYS */;

-- 
-- Definition of orders
-- 

DROP TABLE IF EXISTS `orders`;
CREATE TABLE IF NOT EXISTS `orders` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PaperOrderCode` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CustomerId` int NOT NULL,
  `OrderDate` datetime(6) NOT NULL,
  `DeliveryDeadline` datetime(6) DEFAULT NULL,
  `TotalAmount` decimal(18,2) NOT NULL,
  `DepositAmount` decimal(18,2) NOT NULL,
  `Status` int NOT NULL,
  `IsLocked` tinyint(1) NOT NULL,
  `Notes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DesignFile` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ProductionIndex` int DEFAULT NULL,
  `ProductionStartDate` datetime(6) DEFAULT NULL,
  `ProductionEndDate` datetime(6) DEFAULT NULL,
  `PlanColor` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PaidAmount` decimal(18,2) NOT NULL,
  `PaymentStatus` int NOT NULL,
  `TotalCOGS` decimal(18,2) NOT NULL,
  `RowVersion` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Orders_OrderCode` (`OrderCode`),
  KEY `IX_Orders_CustomerId` (`CustomerId`),
  CONSTRAINT `FK_Orders_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table orders
-- 

/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
INSERT INTO `orders`(`Id`,`OrderCode`,`PaperOrderCode`,`CustomerId`,`OrderDate`,`DeliveryDeadline`,`TotalAmount`,`DepositAmount`,`Status`,`IsLocked`,`Notes`,`DesignFile`,`ProductionIndex`,`ProductionStartDate`,`ProductionEndDate`,`PlanColor`,`PaidAmount`,`PaymentStatus`,`TotalCOGS`,`RowVersion`) VALUES(1,'DH-001',NULL,1,'2026-03-26 21:32:10.247864','2026-03-26 00:00:00.000000',1080000.00,0.00,6,0,NULL,NULL,9999,NULL,NULL,NULL,0.00,0,0.00,'2026-04-04 08:17:53.395238'),(2,'DH-002',NULL,1,'2026-04-15 21:52:09.231044','2026-04-20 00:00:00.000000',16200000.00,0.00,2,0,NULL,NULL,1,NULL,NULL,NULL,0.00,0,0.00,'2026-04-15 14:59:19.168134');
/*!40000 ALTER TABLE `orders` ENABLE KEYS */;

-- 
-- Definition of cashentries
-- 

DROP TABLE IF EXISTS `cashentries`;
CREATE TABLE IF NOT EXISTS `cashentries` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PaperVoucherNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `TransactionDate` datetime(6) NOT NULL,
  `ReportDate` datetime(6) NOT NULL,
  `VoucherCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` int NOT NULL,
  `Method` int NOT NULL,
  `Category` int NOT NULL,
  `Amount` decimal(18,2) NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `TargetName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `OrderId` int DEFAULT NULL,
  `ParentId` int DEFAULT NULL,
  `CreatedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedAt` datetime(6) NOT NULL,
  `MaterialImportId` int DEFAULT NULL,
  `CostCategoryId` int DEFAULT NULL,
  `ForMonth` int DEFAULT NULL,
  `ForYear` int DEFAULT NULL,
  `AllocatedMonths` int DEFAULT NULL,
  `APInvoiceId` int DEFAULT NULL,
  `PurchaseOrderId` int DEFAULT NULL,
  `ExportShipmentId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_CashEntries_APInvoiceId` (`APInvoiceId`),
  KEY `IX_CashEntries_CostCategoryId` (`CostCategoryId`),
  KEY `IX_CashEntries_MaterialImportId` (`MaterialImportId`),
  KEY `IX_CashEntries_OrderId` (`OrderId`),
  KEY `IX_CashEntries_ParentId` (`ParentId`),
  CONSTRAINT `FK_CashEntries_APInvoices_APInvoiceId` FOREIGN KEY (`APInvoiceId`) REFERENCES `apinvoices` (`Id`),
  CONSTRAINT `FK_CashEntries_CashEntries_ParentId` FOREIGN KEY (`ParentId`) REFERENCES `cashentries` (`Id`),
  CONSTRAINT `FK_CashEntries_CostCategories_CostCategoryId` FOREIGN KEY (`CostCategoryId`) REFERENCES `costcategories` (`Id`),
  CONSTRAINT `FK_CashEntries_MaterialImports_MaterialImportId` FOREIGN KEY (`MaterialImportId`) REFERENCES `materialimports` (`Id`),
  CONSTRAINT `FK_CashEntries_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table cashentries
-- 

/*!40000 ALTER TABLE `cashentries` DISABLE KEYS */;
INSERT INTO `cashentries`(`Id`,`PaperVoucherNumber`,`TransactionDate`,`ReportDate`,`VoucherCode`,`Type`,`Method`,`Category`,`Amount`,`Description`,`TargetName`,`OrderId`,`ParentId`,`CreatedBy`,`CreatedAt`,`MaterialImportId`,`CostCategoryId`,`ForMonth`,`ForYear`,`AllocatedMonths`,`APInvoiceId`,`PurchaseOrderId`,`ExportShipmentId`) VALUES(2,'PT-001','2026-04-08 00:00:00.000000','2026-04-08 16:17:35.600125','PT2604-001',1,1,1,250000.00,NULL,'Hảo',NULL,NULL,'sitinhtutu6@gmail.com','2026-04-08 16:17:35.623503',NULL,NULL,NULL,NULL,1,NULL,NULL,NULL);
/*!40000 ALTER TABLE `cashentries` ENABLE KEYS */;

-- 
-- Definition of cashallocations
-- 

DROP TABLE IF EXISTS `cashallocations`;
CREATE TABLE IF NOT EXISTS `cashallocations` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CashEntryId` int NOT NULL,
  `TargetMonth` int NOT NULL,
  `TargetYear` int NOT NULL,
  `AllocatedAmount` decimal(18,2) NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_CashAllocations_CashEntryId` (`CashEntryId`),
  CONSTRAINT `FK_CashAllocations_CashEntries_CashEntryId` FOREIGN KEY (`CashEntryId`) REFERENCES `cashentries` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table cashallocations
-- 

/*!40000 ALTER TABLE `cashallocations` DISABLE KEYS */;
/*!40000 ALTER TABLE `cashallocations` ENABLE KEYS */;

-- 
-- Definition of exportshipmentorders
-- 

DROP TABLE IF EXISTS `exportshipmentorders`;
CREATE TABLE IF NOT EXISTS `exportshipmentorders` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ExportShipmentId` int NOT NULL,
  `OrderId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ExportShipmentOrders_ExportShipmentId` (`ExportShipmentId`),
  KEY `IX_ExportShipmentOrders_OrderId` (`OrderId`),
  CONSTRAINT `FK_ExportShipmentOrders_ExportShipments_ExportShipmentId` FOREIGN KEY (`ExportShipmentId`) REFERENCES `exportshipments` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ExportShipmentOrders_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table exportshipmentorders
-- 

/*!40000 ALTER TABLE `exportshipmentorders` DISABLE KEYS */;
/*!40000 ALTER TABLE `exportshipmentorders` ENABLE KEYS */;

-- 
-- Definition of invoicedetails
-- 

DROP TABLE IF EXISTS `invoicedetails`;
CREATE TABLE IF NOT EXISTS `invoicedetails` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InvoiceId` int NOT NULL,
  `OrderId` int NOT NULL,
  `BilledAmount` decimal(65,30) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_InvoiceDetails_InvoiceId` (`InvoiceId`),
  KEY `IX_InvoiceDetails_OrderId` (`OrderId`),
  CONSTRAINT `FK_InvoiceDetails_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `invoices` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_InvoiceDetails_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table invoicedetails
-- 

/*!40000 ALTER TABLE `invoicedetails` DISABLE KEYS */;
INSERT INTO `invoicedetails`(`Id`,`InvoiceId`,`OrderId`,`BilledAmount`) VALUES(1,1,1,580000.00000000000000000000000);
/*!40000 ALTER TABLE `invoicedetails` ENABLE KEYS */;

-- 
-- Definition of orderdetails
-- 

DROP TABLE IF EXISTS `orderdetails`;
CREATE TABLE IF NOT EXISTS `orderdetails` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderId` int NOT NULL,
  `ProductName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductId` int DEFAULT NULL,
  `Specifications` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Quantity` int NOT NULL,
  `Notes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `UnitPrice` decimal(18,2) NOT NULL,
  `VatPercent` double NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_OrderDetails_OrderId` (`OrderId`),
  CONSTRAINT `FK_OrderDetails_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table orderdetails
-- 

/*!40000 ALTER TABLE `orderdetails` DISABLE KEYS */;
INSERT INTO `orderdetails`(`Id`,`OrderId`,`ProductName`,`ProductId`,`Specifications`,`Quantity`,`Notes`,`UnitPrice`,`VatPercent`) VALUES(1,1,'Ghế KT3',1,'1x1x1',100,NULL,10000.00,8),(2,2,'Ghế KT3',1,'1x1x1',100,NULL,150000.00,8);
/*!40000 ALTER TABLE `orderdetails` ENABLE KEYS */;

-- 
-- Definition of partnercontacts
-- 

DROP TABLE IF EXISTS `partnercontacts`;
CREATE TABLE IF NOT EXISTS `partnercontacts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Company` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Department` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ContactName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Phone` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Zalo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Telegram` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `Status` int NOT NULL,
  `JobDescription` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Notes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table partnercontacts
-- 

/*!40000 ALTER TABLE `partnercontacts` DISABLE KEYS */;
/*!40000 ALTER TABLE `partnercontacts` ENABLE KEYS */;

-- 
-- Definition of purchaseorders
-- 

DROP TABLE IF EXISTS `purchaseorders`;
CREATE TABLE IF NOT EXISTS `purchaseorders` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `POCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `OrderDate` datetime(6) NOT NULL,
  `SupplierId` int NOT NULL,
  `SupplierName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` int NOT NULL,
  `Status` int NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DepositAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `PaymentStatusPO` int NOT NULL DEFAULT '0',
  `RemainingAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `TotalAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `FinalTotal` decimal(18,2) NOT NULL DEFAULT '0.00',
  `VATAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `VATPercent` double NOT NULL DEFAULT '0',
  `DebtFinalizedDate` datetime(6) DEFAULT NULL,
  `InvoiceNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `IsDebtFinalized` tinyint(1) NOT NULL DEFAULT '0',
  `RowVersion` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table purchaseorders
-- 

/*!40000 ALTER TABLE `purchaseorders` DISABLE KEYS */;
INSERT INTO `purchaseorders`(`Id`,`POCode`,`OrderDate`,`SupplierId`,`SupplierName`,`Type`,`Status`,`Note`,`DepositAmount`,`PaymentStatusPO`,`RemainingAmount`,`TotalAmount`,`FinalTotal`,`VATAmount`,`VATPercent`,`DebtFinalizedDate`,`InvoiceNumber`,`IsDebtFinalized`,`RowVersion`) VALUES(1,'PO-260324-417','2026-03-24 00:00:00.000000',1,'CÔNG TY TNHH ABC',2,2,'',550000.00,1,-550000.00,0.00,0.00,0.00,0,'2026-03-25 20:47:05.991242','HD-005',1,'2026-04-04 05:10:25.802314');
/*!40000 ALTER TABLE `purchaseorders` ENABLE KEYS */;

-- 
-- Definition of roles
-- 

DROP TABLE IF EXISTS `roles`;
CREATE TABLE IF NOT EXISTS `roles` (
  `Id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NormalizedName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table roles
-- 

/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles`(`Id`,`Name`,`NormalizedName`,`ConcurrencyStamp`) VALUES('14f5762c-3558-4bf3-bac7-c7b8ba630899','User','USER',NULL),('56dfd853-5756-46fd-95a6-d8c05250a88e','Manager','MANAGER',NULL),('bdb4b9e1-809e-4fe8-b86e-897227fac706','Admin','ADMIN','d0b20ed2-79be-4e98-87a0-bcf2c863d446');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;

-- 
-- Definition of aspnetroleclaims
-- 

DROP TABLE IF EXISTS `aspnetroleclaims`;
CREATE TABLE IF NOT EXISTS `aspnetroleclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetRoleClaims_RoleId` (`RoleId`),
  CONSTRAINT `FK_AspNetRoleClaims_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table aspnetroleclaims
-- 

/*!40000 ALTER TABLE `aspnetroleclaims` DISABLE KEYS */;
INSERT INTO `aspnetroleclaims`(`Id`,`RoleId`,`ClaimType`,`ClaimValue`) VALUES(1,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Products.View'),(2,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Products.Create'),(3,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Products.Edit'),(4,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Products.Delete'),(5,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Orders.View'),(6,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Orders.Create'),(7,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Orders.Edit'),(8,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Orders.Delete'),(9,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Orders.Approve'),(10,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Documents.View'),(11,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Documents.Create'),(12,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Documents.Delete'),(13,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Warehouse.View'),(14,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Warehouse.Create'),(15,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Warehouse.Edit'),(16,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Warehouse.Delete'),(17,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Customers.View'),(18,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Customers.Create'),(19,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Customers.Edit'),(20,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Customers.Delete'),(21,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Users.View'),(22,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Users.Create'),(23,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Users.Edit'),(24,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Users.Delete'),(25,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Roles.View'),(26,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Roles.Create'),(27,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Roles.Edit'),(28,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Roles.Delete'),(29,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Debts.View'),(30,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Debts.Create'),(31,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Debts.Edit'),(32,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Debts.Delete'),(33,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Plans.View'),(34,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Plans.Create'),(35,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Plans.Edit'),(36,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Plans.Delete'),(37,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Norms.View'),(38,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Norms.Create'),(39,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Norms.Edit'),(40,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Norms.Delete'),(41,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Shipments.View'),(42,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Shipments.Create'),(43,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Shipments.Edit'),(44,'bdb4b9e1-809e-4fe8-b86e-897227fac706','Permission','Permissions.Shipments.Delete');
/*!40000 ALTER TABLE `aspnetroleclaims` ENABLE KEYS */;

-- 
-- Definition of shipments
-- 

DROP TABLE IF EXISTS `shipments`;
CREATE TABLE IF NOT EXISTS `shipments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ShipmentCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ShipmentDate` datetime(6) NOT NULL,
  `CustomerId` int NOT NULL,
  `OrderId` int DEFAULT NULL,
  `VehicleNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Notes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ExportShipmentId` int DEFAULT NULL,
  `Type` int NOT NULL,
  `ParentShipmentId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Shipments_CustomerId` (`CustomerId`),
  KEY `IX_Shipments_ExportShipmentId` (`ExportShipmentId`),
  KEY `IX_Shipments_OrderId` (`OrderId`),
  KEY `IX_Shipments_ParentShipmentId` (`ParentShipmentId`),
  CONSTRAINT `FK_Shipments_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Shipments_ExportShipments_ExportShipmentId` FOREIGN KEY (`ExportShipmentId`) REFERENCES `exportshipments` (`Id`),
  CONSTRAINT `FK_Shipments_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`),
  CONSTRAINT `FK_Shipments_Shipments_ParentShipmentId` FOREIGN KEY (`ParentShipmentId`) REFERENCES `shipments` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table shipments
-- 

/*!40000 ALTER TABLE `shipments` DISABLE KEYS */;
INSERT INTO `shipments`(`Id`,`ShipmentCode`,`ShipmentDate`,`CustomerId`,`OrderId`,`VehicleNumber`,`Notes`,`ExportShipmentId`,`Type`,`ParentShipmentId`) VALUES(1,'PX-20260326-5345','2026-03-26 00:00:00.000000',1,1,'51G-531.89',NULL,1,0,NULL);
/*!40000 ALTER TABLE `shipments` ENABLE KEYS */;

-- 
-- Definition of shipmentdetails
-- 

DROP TABLE IF EXISTS `shipmentdetails`;
CREATE TABLE IF NOT EXISTS `shipmentdetails` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ShipmentId` int NOT NULL,
  `OrderDetailId` int NOT NULL,
  `QuantityShipped` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ShipmentDetails_OrderDetailId` (`OrderDetailId`),
  KEY `IX_ShipmentDetails_ShipmentId` (`ShipmentId`),
  CONSTRAINT `FK_ShipmentDetails_OrderDetails_OrderDetailId` FOREIGN KEY (`OrderDetailId`) REFERENCES `orderdetails` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ShipmentDetails_Shipments_ShipmentId` FOREIGN KEY (`ShipmentId`) REFERENCES `shipments` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table shipmentdetails
-- 

/*!40000 ALTER TABLE `shipmentdetails` DISABLE KEYS */;
INSERT INTO `shipmentdetails`(`Id`,`ShipmentId`,`OrderDetailId`,`QuantityShipped`) VALUES(1,1,1,50);
/*!40000 ALTER TABLE `shipmentdetails` ENABLE KEYS */;

-- 
-- Definition of steptrackers
-- 

DROP TABLE IF EXISTS `steptrackers`;
CREATE TABLE IF NOT EXISTS `steptrackers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ExportShipmentId` int NOT NULL,
  `Step` int NOT NULL,
  `Status` int NOT NULL,
  `CompletedDate` datetime(6) DEFAULT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_StepTrackers_ExportShipmentId` (`ExportShipmentId`),
  CONSTRAINT `FK_StepTrackers_ExportShipments_ExportShipmentId` FOREIGN KEY (`ExportShipmentId`) REFERENCES `exportshipments` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table steptrackers
-- 

/*!40000 ALTER TABLE `steptrackers` DISABLE KEYS */;
/*!40000 ALTER TABLE `steptrackers` ENABLE KEYS */;

-- 
-- Definition of systemnotifications
-- 

DROP TABLE IF EXISTS `systemnotifications`;
CREATE TABLE IF NOT EXISTS `systemnotifications` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Message` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StartTime` datetime(6) NOT NULL,
  `EndTime` datetime(6) NOT NULL,
  `IsEnabled` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table systemnotifications
-- 

/*!40000 ALTER TABLE `systemnotifications` DISABLE KEYS */;
/*!40000 ALTER TABLE `systemnotifications` ENABLE KEYS */;

-- 
-- Definition of users
-- 

DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
  `Id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FullName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UserName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Email` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `PasswordHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `SecurityStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEnd` datetime(6) DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
  KEY `EmailIndex` (`NormalizedEmail`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table users
-- 

/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users`(`Id`,`FullName`,`Address`,`IsActive`,`CreatedAt`,`UserName`,`NormalizedUserName`,`Email`,`NormalizedEmail`,`EmailConfirmed`,`PasswordHash`,`SecurityStamp`,`ConcurrencyStamp`,`PhoneNumber`,`PhoneNumberConfirmed`,`TwoFactorEnabled`,`LockoutEnd`,`LockoutEnabled`,`AccessFailedCount`) VALUES('90546b19-e872-48e9-abea-9021c20c4fe2','System Administrator','',1,'2026-03-24 20:13:49.169694','sitinhtutu6@gmail.com','SITINHTUTU6@GMAIL.COM','sitinhtutu6@gmail.com','SITINHTUTU6@GMAIL.COM',1,'AQAAAAIAAYagAAAAED9d4aPm0s8ObC6SOZRIskkzm5Vh6wZm8lDc3KK/VWe5pSead/0gxpyIWblcYCF94g==','5XUBQWYCNVMZMV6GM2F5CDUKYZ6Z4GDN','943515bd-89bd-48f1-9e2f-9f149f4cbb62',NULL,0,0,NULL,1,0);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;

-- 
-- Definition of aspnetuserlogins
-- 

DROP TABLE IF EXISTS `aspnetuserlogins`;
CREATE TABLE IF NOT EXISTS `aspnetuserlogins` (
  `LoginProvider` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderKey` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderDisplayName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`),
  KEY `IX_AspNetUserLogins_UserId` (`UserId`),
  CONSTRAINT `FK_AspNetUserLogins_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table aspnetuserlogins
-- 

/*!40000 ALTER TABLE `aspnetuserlogins` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserlogins` ENABLE KEYS */;

-- 
-- Definition of aspnetuserroles
-- 

DROP TABLE IF EXISTS `aspnetuserroles`;
CREATE TABLE IF NOT EXISTS `aspnetuserroles` (
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RoleId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IX_AspNetUserRoles_RoleId` (`RoleId`),
  CONSTRAINT `FK_AspNetUserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AspNetUserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table aspnetuserroles
-- 

/*!40000 ALTER TABLE `aspnetuserroles` DISABLE KEYS */;
INSERT INTO `aspnetuserroles`(`UserId`,`RoleId`) VALUES('90546b19-e872-48e9-abea-9021c20c4fe2','bdb4b9e1-809e-4fe8-b86e-897227fac706');
/*!40000 ALTER TABLE `aspnetuserroles` ENABLE KEYS */;

-- 
-- Definition of aspnetusertokens
-- 

DROP TABLE IF EXISTS `aspnetusertokens`;
CREATE TABLE IF NOT EXISTS `aspnetusertokens` (
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LoginProvider` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`UserId`,`LoginProvider`,`Name`),
  CONSTRAINT `FK_AspNetUserTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table aspnetusertokens
-- 

/*!40000 ALTER TABLE `aspnetusertokens` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetusertokens` ENABLE KEYS */;

-- 
-- Definition of userclaims
-- 

DROP TABLE IF EXISTS `userclaims`;
CREATE TABLE IF NOT EXISTS `userclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_UserClaims_UserId` (`UserId`),
  CONSTRAINT `FK_UserClaims_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table userclaims
-- 

/*!40000 ALTER TABLE `userclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `userclaims` ENABLE KEYS */;

-- 
-- Definition of warehousecategories
-- 

DROP TABLE IF EXISTS `warehousecategories`;
CREATE TABLE IF NOT EXISTS `warehousecategories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table warehousecategories
-- 

/*!40000 ALTER TABLE `warehousecategories` DISABLE KEYS */;
INSERT INTO `warehousecategories`(`Id`,`Name`) VALUES(1,'Nguyên vật liệu'),(2,'Bán thành phẩm'),(3,'Thành phẩm'),(4,'Gỗ'),(5,'Màu');
/*!40000 ALTER TABLE `warehousecategories` ENABLE KEYS */;

-- 
-- Definition of warehouseitems
-- 

DROP TABLE IF EXISTS `warehouseitems`;
CREATE TABLE IF NOT EXISTS `warehouseitems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CategoryId` int DEFAULT NULL,
  `Unit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StockQuantity` double NOT NULL,
  `CostPrice` decimal(18,2) NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Image` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ItemType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LinkedProductId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_WarehouseItems_Code` (`Code`),
  KEY `IX_WarehouseItems_CategoryId` (`CategoryId`),
  CONSTRAINT `FK_WarehouseItems_WarehouseCategories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `warehousecategories` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table warehouseitems
-- 

/*!40000 ALTER TABLE `warehouseitems` DISABLE KEYS */;
INSERT INTO `warehouseitems`(`Id`,`Code`,`Name`,`CategoryId`,`Unit`,`StockQuantity`,`CostPrice`,`Note`,`Image`,`ItemType`,`LinkedProductId`) VALUES(1,'VT-0001','Gỗ ASH',4,'Cái',10,0.00,NULL,NULL,'Material',NULL),(2,'SP-0001','Ghế KT3',3,'Cái',0,10000.00,'Được tạo tự động từ Module Sản Phẩm',NULL,'Product',1);
/*!40000 ALTER TABLE `warehouseitems` ENABLE KEYS */;

-- 
-- Definition of materials
-- 

DROP TABLE IF EXISTS `materials`;
CREATE TABLE IF NOT EXISTS `materials` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `WarehouseItemId` int NOT NULL,
  `Quantity` double NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_Materials_ProductId` (`ProductId`),
  KEY `IX_Materials_WarehouseItemId` (`WarehouseItemId`),
  CONSTRAINT `FK_Materials_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Materials_WarehouseItems_WarehouseItemId` FOREIGN KEY (`WarehouseItemId`) REFERENCES `warehouseitems` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table materials
-- 

/*!40000 ALTER TABLE `materials` DISABLE KEYS */;
/*!40000 ALTER TABLE `materials` ENABLE KEYS */;

-- 
-- Definition of productboms
-- 

DROP TABLE IF EXISTS `productboms`;
CREATE TABLE IF NOT EXISTS `productboms` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `MaterialId` int NOT NULL,
  `ComponentName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Length` double NOT NULL,
  `Width` double NOT NULL,
  `Height` double NOT NULL,
  `CutQuantity` double NOT NULL,
  `Coefficient` double NOT NULL,
  `Quantity` double NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_ProductBoms_MaterialId` (`MaterialId`),
  KEY `IX_ProductBoms_ProductId` (`ProductId`),
  CONSTRAINT `FK_ProductBoms_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ProductBoms_WarehouseItems_MaterialId` FOREIGN KEY (`MaterialId`) REFERENCES `warehouseitems` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table productboms
-- 

/*!40000 ALTER TABLE `productboms` DISABLE KEYS */;
/*!40000 ALTER TABLE `productboms` ENABLE KEYS */;

-- 
-- Definition of products
-- 

DROP TABLE IF EXISTS `products`;
CREATE TABLE IF NOT EXISTS `products` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Color` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `MaterialType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Unit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Thick` double NOT NULL,
  `Width` double NOT NULL,
  `Length` double NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `HasVariants` tinyint(1) NOT NULL,
  `DefaultPrice` decimal(18,2) NOT NULL,
  `ImagePath` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `WarehouseItemId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Products_ProductCode` (`ProductCode`),
  KEY `IX_Products_WarehouseItemId` (`WarehouseItemId`),
  CONSTRAINT `FK_Products_WarehouseItems_WarehouseItemId` FOREIGN KEY (`WarehouseItemId`) REFERENCES `warehouseitems` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table products
-- 

/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products`(`Id`,`ProductCode`,`ProductName`,`Color`,`MaterialType`,`Unit`,`Thick`,`Width`,`Length`,`Note`,`HasVariants`,`DefaultPrice`,`ImagePath`,`WarehouseItemId`) VALUES(1,'SP-0001','Ghế KT3','Xanh','Beech','Cái',1,1,1,NULL,1,10000.00,NULL,2);
/*!40000 ALTER TABLE `products` ENABLE KEYS */;

-- 
-- Definition of productdetails
-- 

DROP TABLE IF EXISTS `productdetails`;
CREATE TABLE IF NOT EXISTS `productdetails` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `DetailCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `VariantName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Color` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `MaterialType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Thick` double NOT NULL,
  `Width` double NOT NULL,
  `Length` double NOT NULL,
  `Quantity` int NOT NULL,
  `Price` decimal(18,2) NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `WarehouseItemId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ProductDetails_ProductId` (`ProductId`),
  CONSTRAINT `FK_ProductDetails_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table productdetails
-- 

/*!40000 ALTER TABLE `productdetails` DISABLE KEYS */;
/*!40000 ALTER TABLE `productdetails` ENABLE KEYS */;

-- 
-- Definition of productdocuments
-- 

DROP TABLE IF EXISTS `productdocuments`;
CREATE TABLE IF NOT EXISTS `productdocuments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `FilePath` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `FileExtension` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedDate` datetime(6) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ProductDocuments_ProductId` (`ProductId`),
  CONSTRAINT `FK_ProductDocuments_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table productdocuments
-- 

/*!40000 ALTER TABLE `productdocuments` DISABLE KEYS */;
/*!40000 ALTER TABLE `productdocuments` ENABLE KEYS */;

-- 
-- Definition of productpricehistories
-- 

DROP TABLE IF EXISTS `productpricehistories`;
CREATE TABLE IF NOT EXISTS `productpricehistories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `ProductDetailId` int DEFAULT NULL,
  `OldPrice` decimal(18,2) NOT NULL,
  `NewPrice` decimal(18,2) NOT NULL,
  `ChangedDate` datetime(6) NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_ProductPriceHistories_ProductId` (`ProductId`),
  CONSTRAINT `FK_ProductPriceHistories_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table productpricehistories
-- 

/*!40000 ALTER TABLE `productpricehistories` DISABLE KEYS */;
/*!40000 ALTER TABLE `productpricehistories` ENABLE KEYS */;

-- 
-- Definition of purchaseorderdetails
-- 

DROP TABLE IF EXISTS `purchaseorderdetails`;
CREATE TABLE IF NOT EXISTS `purchaseorderdetails` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PurchaseOrderId` int NOT NULL,
  `WarehouseItemId` int NOT NULL,
  `QuantityOrdered` double NOT NULL,
  `QuantityReceived` double NOT NULL,
  `QuantityBad` double NOT NULL,
  `UnitPrice` decimal(18,2) NOT NULL,
  `IsFinished` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_PurchaseOrderDetails_PurchaseOrderId` (`PurchaseOrderId`),
  KEY `IX_PurchaseOrderDetails_WarehouseItemId` (`WarehouseItemId`),
  CONSTRAINT `FK_PurchaseOrderDetails_PurchaseOrders_PurchaseOrderId` FOREIGN KEY (`PurchaseOrderId`) REFERENCES `purchaseorders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_PurchaseOrderDetails_WarehouseItems_WarehouseItemId` FOREIGN KEY (`WarehouseItemId`) REFERENCES `warehouseitems` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table purchaseorderdetails
-- 

/*!40000 ALTER TABLE `purchaseorderdetails` DISABLE KEYS */;
INSERT INTO `purchaseorderdetails`(`Id`,`PurchaseOrderId`,`WarehouseItemId`,`QuantityOrdered`,`QuantityReceived`,`QuantityBad`,`UnitPrice`,`IsFinished`) VALUES(1,1,1,10,10,0,0.00,1);
/*!40000 ALTER TABLE `purchaseorderdetails` ENABLE KEYS */;

-- 
-- Definition of stocktransactions
-- 

DROP TABLE IF EXISTS `stocktransactions`;
CREATE TABLE IF NOT EXISTS `stocktransactions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `WarehouseItemId` int NOT NULL,
  `TransactionDate` datetime(6) NOT NULL,
  `Type` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Quantity` double NOT NULL,
  `CurrentStock` double NOT NULL,
  `Price` decimal(18,2) NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DocumentCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Receiver` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Staff` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `OrderId` int DEFAULT NULL,
  `Reason` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_StockTransactions_OrderId` (`OrderId`),
  KEY `IX_StockTransactions_WarehouseItemId` (`WarehouseItemId`),
  CONSTRAINT `FK_StockTransactions_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`),
  CONSTRAINT `FK_StockTransactions_WarehouseItems_WarehouseItemId` FOREIGN KEY (`WarehouseItemId`) REFERENCES `warehouseitems` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table stocktransactions
-- 

/*!40000 ALTER TABLE `stocktransactions` DISABLE KEYS */;
INSERT INTO `stocktransactions`(`Id`,`WarehouseItemId`,`TransactionDate`,`Type`,`Quantity`,`CurrentStock`,`Price`,`Note`,`DocumentCode`,`Receiver`,`Staff`,`OrderId`,`Reason`) VALUES(1,1,'2026-03-25 12:20:27.621000','Import',10,10,0.00,'','PN-20260325-1920','CÔNG TY TNHH ABC','sitinhtutu6@gmail.com',NULL,NULL);
/*!40000 ALTER TABLE `stocktransactions` ENABLE KEYS */;

-- 
-- Definition of warehousetickets
-- 

DROP TABLE IF EXISTS `warehousetickets`;
CREATE TABLE IF NOT EXISTS `warehousetickets` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TicketCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` int NOT NULL,
  `Date` datetime(6) NOT NULL,
  `Subject` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatorId` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table warehousetickets
-- 

/*!40000 ALTER TABLE `warehousetickets` DISABLE KEYS */;
/*!40000 ALTER TABLE `warehousetickets` ENABLE KEYS */;

-- 
-- Definition of warehouseticketdetails
-- 

DROP TABLE IF EXISTS `warehouseticketdetails`;
CREATE TABLE IF NOT EXISTS `warehouseticketdetails` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `WarehouseTicketId` int NOT NULL,
  `ProductDetailId` int NOT NULL,
  `Quantity` int NOT NULL,
  `SystemStock` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_WarehouseTicketDetails_ProductDetailId` (`ProductDetailId`),
  KEY `IX_WarehouseTicketDetails_WarehouseTicketId` (`WarehouseTicketId`),
  CONSTRAINT `FK_WarehouseTicketDetails_ProductDetails_ProductDetailId` FOREIGN KEY (`ProductDetailId`) REFERENCES `productdetails` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_WarehouseTicketDetails_WarehouseTickets_WarehouseTicketId` FOREIGN KEY (`WarehouseTicketId`) REFERENCES `warehousetickets` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table warehouseticketdetails
-- 

/*!40000 ALTER TABLE `warehouseticketdetails` DISABLE KEYS */;
/*!40000 ALTER TABLE `warehouseticketdetails` ENABLE KEYS */;

-- 
-- Definition of workorder
-- 

DROP TABLE IF EXISTS `workorder`;
CREATE TABLE IF NOT EXISTS `workorder` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderId` int NOT NULL,
  `WOCode` varchar(50) NOT NULL,
  `ProductName` varchar(255) DEFAULT NULL,
  `TargetQuantity` int DEFAULT '0',
  `FinishedQuantity` int DEFAULT '0',
  `StartDate` datetime DEFAULT NULL,
  `EndDate` datetime DEFAULT NULL,
  `Status` int DEFAULT '0',
  `MachineId` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_WorkOrder_Order` (`OrderId`),
  CONSTRAINT `FK_WorkOrder_Order` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table workorder
-- 

/*!40000 ALTER TABLE `workorder` DISABLE KEYS */;
/*!40000 ALTER TABLE `workorder` ENABLE KEYS */;


/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;


-- Dump completed on 2026-04-16 21:46:37
-- Total time: 0:0:0:0:490 (d:h:m:s:ms)
