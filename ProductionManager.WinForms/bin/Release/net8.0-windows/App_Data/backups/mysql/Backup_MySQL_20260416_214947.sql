-- MySqlBackup.NET 2.6.5.0
-- Dump Time: 2026-04-16 21:49:47
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
INSERT INTO `__efmigrationshistory`(`MigrationId`,`ProductVersion`) VALUES('20260414042600_InitialCreate','8.0.11'),('20260415154523_InitialAddHistory','8.0.11'),('20260416073720_AddWorkOrderTable','8.0.11'),('20260416074815_UpdateWorkOrderTable','8.0.11'),('20260416144454_Updateworkorder2','8.0.11');
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
) ENGINE=InnoDB AUTO_INCREMENT=96 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table auditlogs
-- 

/*!40000 ALTER TABLE `auditlogs` DISABLE KEYS */;
INSERT INTO `auditlogs`(`Id`,`UserName`,`Action`,`TableName`,`RecordId`,`OldValues`,`NewValues`,`Timestamp`) VALUES(1,'Hệ thống','Added','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17',NULL,'{\"Id\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.502097'),(2,'Hệ thống','Added','IdentityRole','be99bcb7-b461-44ed-8172-97174d621eba',NULL,'{\"Id\":\"be99bcb7-b461-44ed-8172-97174d621eba\",\"Name\":\"Manager\",\"NormalizedName\":\"MANAGER\"}','2026-04-16 21:48:37.599362'),(3,'Hệ thống','Added','IdentityRole','55d9e670-e2ce-44c8-a5db-f79749b09c9d',NULL,'{\"Id\":\"55d9e670-e2ce-44c8-a5db-f79749b09c9d\",\"Name\":\"User\",\"NormalizedName\":\"USER\"}','2026-04-16 21:48:37.612128'),(4,'Hệ thống','Added','IdentityRoleClaim`1','-2147482647',NULL,'{\"Id\":-2147482647,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Products.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.659639'),(5,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"0540d542-e40c-4beb-b375-2e7713f08606\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.662129'),(6,'Hệ thống','Added','IdentityRoleClaim`1','-2147482646',NULL,'{\"Id\":-2147482646,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Products.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.681670'),(7,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"0540d542-e40c-4beb-b375-2e7713f08606\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"15edde14-1f48-49d3-8ae9-d35d48a8e9ad\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.681719'),(8,'Hệ thống','Added','IdentityRoleClaim`1','-2147482645',NULL,'{\"Id\":-2147482645,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Products.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.694603'),(9,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"15edde14-1f48-49d3-8ae9-d35d48a8e9ad\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"6bfb40a0-d6d6-4f78-86bc-73293232a36b\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.694649'),(10,'Hệ thống','Added','IdentityRoleClaim`1','-2147482644',NULL,'{\"Id\":-2147482644,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Products.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.706771'),(11,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"6bfb40a0-d6d6-4f78-86bc-73293232a36b\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"c788f8eb-6de6-4689-9b87-b29ef7d7deed\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.706821'),(12,'Hệ thống','Added','IdentityRoleClaim`1','-2147482643',NULL,'{\"Id\":-2147482643,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.719105'),(13,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"c788f8eb-6de6-4689-9b87-b29ef7d7deed\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"659659a8-9625-40b6-bfce-21dcceea6bff\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.719150'),(14,'Hệ thống','Added','IdentityRoleClaim`1','-2147482642',NULL,'{\"Id\":-2147482642,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.731018'),(15,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"659659a8-9625-40b6-bfce-21dcceea6bff\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"168d3825-6779-4953-ac4d-0915f8545889\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.731065'),(16,'Hệ thống','Added','IdentityRoleClaim`1','-2147482641',NULL,'{\"Id\":-2147482641,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.742905'),(17,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"168d3825-6779-4953-ac4d-0915f8545889\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"f520a0f8-fb83-4363-bfa6-8a1cc0c2d6e2\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.742960'),(18,'Hệ thống','Added','IdentityRoleClaim`1','-2147482640',NULL,'{\"Id\":-2147482640,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.756859'),(19,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"f520a0f8-fb83-4363-bfa6-8a1cc0c2d6e2\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"9ae0f9ac-ce70-4d70-a588-61e6d1c0ce65\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.756908'),(20,'Hệ thống','Added','IdentityRoleClaim`1','-2147482639',NULL,'{\"Id\":-2147482639,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Orders.Approve\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.768510'),(21,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"9ae0f9ac-ce70-4d70-a588-61e6d1c0ce65\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"95142c88-6c73-461c-a084-f40463689a02\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.768556'),(22,'Hệ thống','Added','IdentityRoleClaim`1','-2147482638',NULL,'{\"Id\":-2147482638,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Documents.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.779311'),(23,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"95142c88-6c73-461c-a084-f40463689a02\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"dbbb1484-bc93-4a7f-9215-84b4af7809c3\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.779360'),(24,'Hệ thống','Added','IdentityRoleClaim`1','-2147482637',NULL,'{\"Id\":-2147482637,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Documents.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.791821'),(25,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"dbbb1484-bc93-4a7f-9215-84b4af7809c3\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"93a23001-a287-4bac-9319-3a2fdcba4b93\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.791865'),(26,'Hệ thống','Added','IdentityRoleClaim`1','-2147482636',NULL,'{\"Id\":-2147482636,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Documents.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.802364'),(27,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"93a23001-a287-4bac-9319-3a2fdcba4b93\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"3888ed9e-3e1a-4be8-a815-69ef8b079028\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.802412'),(28,'Hệ thống','Added','IdentityRoleClaim`1','-2147482635',NULL,'{\"Id\":-2147482635,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Warehouse.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.818165'),(29,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"3888ed9e-3e1a-4be8-a815-69ef8b079028\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"10a660ea-ff44-4696-b70d-467d312efbd2\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.818222'),(30,'Hệ thống','Added','IdentityRoleClaim`1','-2147482634',NULL,'{\"Id\":-2147482634,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Warehouse.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.830731'),(31,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"10a660ea-ff44-4696-b70d-467d312efbd2\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"e389ea1a-5d40-46d5-b534-261ec67be7ab\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.830781'),(32,'Hệ thống','Added','IdentityRoleClaim`1','-2147482633',NULL,'{\"Id\":-2147482633,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Warehouse.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.842937'),(33,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"e389ea1a-5d40-46d5-b534-261ec67be7ab\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"abeb5feb-4455-4010-8de2-e7cdf43e6a4e\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.842984'),(34,'Hệ thống','Added','IdentityRoleClaim`1','-2147482632',NULL,'{\"Id\":-2147482632,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Warehouse.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.854656'),(35,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"abeb5feb-4455-4010-8de2-e7cdf43e6a4e\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"6358dd83-3385-41a8-a9ea-b1b06671c9cd\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.854702'),(36,'Hệ thống','Added','IdentityRoleClaim`1','-2147482631',NULL,'{\"Id\":-2147482631,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Customers.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.868607'),(37,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"6358dd83-3385-41a8-a9ea-b1b06671c9cd\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"a8d83702-8a95-47ab-b52f-389aaec09165\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.868652'),(38,'Hệ thống','Added','IdentityRoleClaim`1','-2147482630',NULL,'{\"Id\":-2147482630,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Customers.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.880623'),(39,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"a8d83702-8a95-47ab-b52f-389aaec09165\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"df3da8db-717c-4fa7-b8f2-8759e71ad796\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.880665'),(40,'Hệ thống','Added','IdentityRoleClaim`1','-2147482629',NULL,'{\"Id\":-2147482629,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Customers.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.893973'),(41,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"df3da8db-717c-4fa7-b8f2-8759e71ad796\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"356fdeec-b0a4-4358-8cbe-d334badc047a\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.894015'),(42,'Hệ thống','Added','IdentityRoleClaim`1','-2147482628',NULL,'{\"Id\":-2147482628,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Customers.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.905233'),(43,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"356fdeec-b0a4-4358-8cbe-d334badc047a\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"cffda07d-8b63-46c6-be20-5d59077e6619\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.905281'),(44,'Hệ thống','Added','IdentityRoleClaim`1','-2147482627',NULL,'{\"Id\":-2147482627,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Users.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.918182'),(45,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"cffda07d-8b63-46c6-be20-5d59077e6619\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"57ee3218-7187-4d4d-bdb5-5d178eff2893\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.918228'),(46,'Hệ thống','Added','IdentityRoleClaim`1','-2147482626',NULL,'{\"Id\":-2147482626,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Users.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.931236'),(47,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"57ee3218-7187-4d4d-bdb5-5d178eff2893\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"e35f1082-a72f-4a50-9bcb-d2a132f64d82\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.931278'),(48,'Hệ thống','Added','IdentityRoleClaim`1','-2147482625',NULL,'{\"Id\":-2147482625,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Users.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.946261'),(49,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"e35f1082-a72f-4a50-9bcb-d2a132f64d82\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"3c56f226-9f41-4ab4-8050-875bf494b5d6\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.946302'),(50,'Hệ thống','Added','IdentityRoleClaim`1','-2147482624',NULL,'{\"Id\":-2147482624,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Users.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.959241'),(51,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"3c56f226-9f41-4ab4-8050-875bf494b5d6\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"5883de9c-e145-4cb5-a18d-5778317ac0cb\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.959285'),(52,'Hệ thống','Added','IdentityRoleClaim`1','-2147482623',NULL,'{\"Id\":-2147482623,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Roles.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.973276'),(53,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"5883de9c-e145-4cb5-a18d-5778317ac0cb\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"6daae5ff-6253-42c8-9b20-cc8a68c5f776\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.973318'),(54,'Hệ thống','Added','IdentityRoleClaim`1','-2147482622',NULL,'{\"Id\":-2147482622,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Roles.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:37.986943'),(55,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"6daae5ff-6253-42c8-9b20-cc8a68c5f776\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"f478ff18-89ab-4c33-90ee-9fe5246be461\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:37.986988'),(56,'Hệ thống','Added','IdentityRoleClaim`1','-2147482621',NULL,'{\"Id\":-2147482621,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Roles.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.000103'),(57,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"f478ff18-89ab-4c33-90ee-9fe5246be461\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"53a81d3b-34db-448b-8a5d-7c70e7cf43c8\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.000147'),(58,'Hệ thống','Added','IdentityRoleClaim`1','-2147482620',NULL,'{\"Id\":-2147482620,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Roles.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.013452'),(59,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"53a81d3b-34db-448b-8a5d-7c70e7cf43c8\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"c7563d0f-917e-4a0c-a38c-633f7685b090\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.013494'),(60,'Hệ thống','Added','IdentityRoleClaim`1','-2147482619',NULL,'{\"Id\":-2147482619,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Debts.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.026262'),(61,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"c7563d0f-917e-4a0c-a38c-633f7685b090\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"69158d9d-46c5-430d-96b5-b28af2515c80\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.026304'),(62,'Hệ thống','Added','IdentityRoleClaim`1','-2147482618',NULL,'{\"Id\":-2147482618,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Debts.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.040250'),(63,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"69158d9d-46c5-430d-96b5-b28af2515c80\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"84802b5e-4e9b-459d-94fb-3647494659ac\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.040291'),(64,'Hệ thống','Added','IdentityRoleClaim`1','-2147482617',NULL,'{\"Id\":-2147482617,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Debts.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.052828'),(65,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"84802b5e-4e9b-459d-94fb-3647494659ac\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"6dc7032d-b281-4b1c-a8be-9838bdc6fc9f\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.052867'),(66,'Hệ thống','Added','IdentityRoleClaim`1','-2147482616',NULL,'{\"Id\":-2147482616,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Debts.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.082371'),(67,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"6dc7032d-b281-4b1c-a8be-9838bdc6fc9f\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"a95f9359-51a2-480e-9884-de6aa5421804\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.082421'),(68,'Hệ thống','Added','IdentityRoleClaim`1','-2147482615',NULL,'{\"Id\":-2147482615,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Plans.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.095455'),(69,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"a95f9359-51a2-480e-9884-de6aa5421804\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"88d86a75-9a7e-4351-8df7-c35fab394e61\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.095491'),(70,'Hệ thống','Added','IdentityRoleClaim`1','-2147482614',NULL,'{\"Id\":-2147482614,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Plans.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.108371'),(71,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"88d86a75-9a7e-4351-8df7-c35fab394e61\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"76e762a5-6655-4022-b31b-694d37ab3d37\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.108408'),(72,'Hệ thống','Added','IdentityRoleClaim`1','-2147482613',NULL,'{\"Id\":-2147482613,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Plans.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.121145'),(73,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"76e762a5-6655-4022-b31b-694d37ab3d37\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"6a59f7fe-a8d3-48ca-bf86-fc38b5c9ab42\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.121181'),(74,'Hệ thống','Added','IdentityRoleClaim`1','-2147482612',NULL,'{\"Id\":-2147482612,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Plans.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.132053'),(75,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"6a59f7fe-a8d3-48ca-bf86-fc38b5c9ab42\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"da661209-5533-4cd5-8569-39fac97f0aa8\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.132088'),(76,'Hệ thống','Added','IdentityRoleClaim`1','-2147482611',NULL,'{\"Id\":-2147482611,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Norms.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.144934'),(77,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"da661209-5533-4cd5-8569-39fac97f0aa8\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"e3a0aca9-8eb7-4eeb-af8b-2b8a8b9ef89a\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.144965'),(78,'Hệ thống','Added','IdentityRoleClaim`1','-2147482610',NULL,'{\"Id\":-2147482610,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Norms.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.158128'),(79,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"e3a0aca9-8eb7-4eeb-af8b-2b8a8b9ef89a\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"f6ba04ff-7f44-4308-908e-4c981f6606a8\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.158162'),(80,'Hệ thống','Added','IdentityRoleClaim`1','-2147482609',NULL,'{\"Id\":-2147482609,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Norms.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.169591'),(81,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"f6ba04ff-7f44-4308-908e-4c981f6606a8\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"23ef9449-3b50-4639-ac15-21c2f906df60\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.169629'),(82,'Hệ thống','Added','IdentityRoleClaim`1','-2147482608',NULL,'{\"Id\":-2147482608,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Norms.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.183583'),(83,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"23ef9449-3b50-4639-ac15-21c2f906df60\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"90db4be6-ff83-4c3b-8068-e6d64e7cf443\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.183620'),(84,'Hệ thống','Added','IdentityRoleClaim`1','-2147482607',NULL,'{\"Id\":-2147482607,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Shipments.View\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.197735'),(85,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"90db4be6-ff83-4c3b-8068-e6d64e7cf443\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"72a27227-ee01-4fc4-a93e-ac51c2af95b9\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.197771'),(86,'Hệ thống','Added','IdentityRoleClaim`1','-2147482606',NULL,'{\"Id\":-2147482606,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Shipments.Create\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.209724'),(87,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"72a27227-ee01-4fc4-a93e-ac51c2af95b9\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"29da5cc9-7eec-4ebf-afbf-f69bdd3c0438\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.209763'),(88,'Hệ thống','Added','IdentityRoleClaim`1','-2147482605',NULL,'{\"Id\":-2147482605,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Shipments.Edit\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.220307'),(89,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"29da5cc9-7eec-4ebf-afbf-f69bdd3c0438\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"8af59659-fcb9-486f-96bc-1533b7709885\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.220339'),(90,'Hệ thống','Added','IdentityRoleClaim`1','-2147482604',NULL,'{\"Id\":-2147482604,\"ClaimType\":\"Permission\",\"ClaimValue\":\"Permissions.Shipments.Delete\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.232866'),(91,'Hệ thống','Modified','IdentityRole','56b09bb8-7e2d-42bc-b34d-f477fec9ce17','{\"ConcurrencyStamp\":\"8af59659-fcb9-486f-96bc-1533b7709885\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','{\"ConcurrencyStamp\":\"dcff88b3-2ef3-4ed5-868f-b68514325aa3\",\"Name\":\"Admin\",\"NormalizedName\":\"ADMIN\"}','2026-04-16 21:48:38.232904'),(92,'Hệ thống','Added','AppUser','e89e777c-547c-48a7-96a2-d320e2128e52',NULL,'{\"Id\":\"e89e777c-547c-48a7-96a2-d320e2128e52\",\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"c2db7527-f6f2-477d-98ad-37562edd1be4\",\"CreatedAt\":\"2026-04-16T21:48:38.2438638+07:00\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAEOr7WM7LaWQYT59HHA3Z6W6U12JosBqwOwPFtexF7Ebe6ehQO3/rqIePfrhtn6Hnvg==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"PUKICJFLJ62SE7BN7T5AXP7F64YJ5VGU\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-04-16 21:48:38.336896'),(93,'Hệ thống','Added','IdentityUserRole`1','56b09bb8-7e2d-42bc-b34d-f477fec9ce17',NULL,'{\"UserId\":\"e89e777c-547c-48a7-96a2-d320e2128e52\",\"RoleId\":\"56b09bb8-7e2d-42bc-b34d-f477fec9ce17\"}','2026-04-16 21:48:38.387798'),(94,'Hệ thống','Modified','AppUser','e89e777c-547c-48a7-96a2-d320e2128e52','{\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"c2db7527-f6f2-477d-98ad-37562edd1be4\",\"CreatedAt\":\"2026-04-16T21:48:38.2438638+07:00\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAEOr7WM7LaWQYT59HHA3Z6W6U12JosBqwOwPFtexF7Ebe6ehQO3/rqIePfrhtn6Hnvg==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"PUKICJFLJ62SE7BN7T5AXP7F64YJ5VGU\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','{\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"e3234456-d604-47f2-a214-3b99c675b04c\",\"CreatedAt\":\"2026-04-16T21:48:38.2438638+07:00\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAEOr7WM7LaWQYT59HHA3Z6W6U12JosBqwOwPFtexF7Ebe6ehQO3/rqIePfrhtn6Hnvg==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"PUKICJFLJ62SE7BN7T5AXP7F64YJ5VGU\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-04-16 21:48:38.387834'),(95,'Hệ thống','Modified','AppUser','e89e777c-547c-48a7-96a2-d320e2128e52','{\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"e3234456-d604-47f2-a214-3b99c675b04c\",\"CreatedAt\":\"2026-04-16T21:48:38.243863\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAEOr7WM7LaWQYT59HHA3Z6W6U12JosBqwOwPFtexF7Ebe6ehQO3/rqIePfrhtn6Hnvg==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"PUKICJFLJ62SE7BN7T5AXP7F64YJ5VGU\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','{\"AccessFailedCount\":0,\"Address\":\"\",\"ConcurrencyStamp\":\"75f04eec-90f5-49e7-980e-09959ecd75bc\",\"CreatedAt\":\"2026-04-16T21:48:38.243863\",\"Email\":\"sitinhtutu6@gmail.com\",\"EmailConfirmed\":true,\"FullName\":\"System Administrator\",\"IsActive\":true,\"LockoutEnabled\":true,\"NormalizedEmail\":\"SITINHTUTU6@GMAIL.COM\",\"NormalizedUserName\":\"SITINHTUTU6@GMAIL.COM\",\"PasswordHash\":\"AQAAAAIAAYagAAAAEIIsgyAKopZiL\\u002BVYRwRRtf/EqWA/ysryIVsb6COqo2XedxNNibD\\u002B2fPYsBT9jR0mTg==\",\"PhoneNumberConfirmed\":false,\"SecurityStamp\":\"36NYOHUWPWXTGFMOJYC5CSDLYXNGXBN4\",\"TwoFactorEnabled\":false,\"UserName\":\"sitinhtutu6@gmail.com\"}','2026-04-16 21:49:30.966091');
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table customers
-- 

/*!40000 ALTER TABLE `customers` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table exportshipments
-- 

/*!40000 ALTER TABLE `exportshipments` DISABLE KEYS */;
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
  `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table invoices
-- 

/*!40000 ALTER TABLE `invoices` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table materialimports
-- 

/*!40000 ALTER TABLE `materialimports` DISABLE KEYS */;
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
  `Action` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `OldValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `NewValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ChangedAt` datetime(6) NOT NULL,
  `ChangedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
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
  `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Orders_OrderCode` (`OrderCode`),
  KEY `IX_Orders_CustomerId` (`CustomerId`),
  CONSTRAINT `FK_Orders_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table orders
-- 

/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table cashentries
-- 

/*!40000 ALTER TABLE `cashentries` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table invoicedetails
-- 

/*!40000 ALTER TABLE `invoicedetails` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table orderdetails
-- 

/*!40000 ALTER TABLE `orderdetails` DISABLE KEYS */;
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
  `TotalAmount` decimal(18,2) NOT NULL,
  `VATPercent` double NOT NULL,
  `VATAmount` decimal(18,2) NOT NULL,
  `FinalTotal` decimal(18,2) NOT NULL,
  `DepositAmount` decimal(18,2) NOT NULL,
  `RemainingAmount` decimal(18,2) NOT NULL,
  `PaymentStatusPO` int NOT NULL,
  `InvoiceNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `IsDebtFinalized` tinyint(1) NOT NULL,
  `DebtFinalizedDate` datetime(6) DEFAULT NULL,
  `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table purchaseorders
-- 

/*!40000 ALTER TABLE `purchaseorders` DISABLE KEYS */;
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
INSERT INTO `roles`(`Id`,`Name`,`NormalizedName`,`ConcurrencyStamp`) VALUES('55d9e670-e2ce-44c8-a5db-f79749b09c9d','User','USER',NULL),('56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Admin','ADMIN','dcff88b3-2ef3-4ed5-868f-b68514325aa3'),('be99bcb7-b461-44ed-8172-97174d621eba','Manager','MANAGER',NULL);
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
INSERT INTO `aspnetroleclaims`(`Id`,`RoleId`,`ClaimType`,`ClaimValue`) VALUES(1,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Products.View'),(2,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Products.Create'),(3,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Products.Edit'),(4,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Products.Delete'),(5,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Orders.View'),(6,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Orders.Create'),(7,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Orders.Edit'),(8,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Orders.Delete'),(9,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Orders.Approve'),(10,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Documents.View'),(11,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Documents.Create'),(12,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Documents.Delete'),(13,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Warehouse.View'),(14,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Warehouse.Create'),(15,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Warehouse.Edit'),(16,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Warehouse.Delete'),(17,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Customers.View'),(18,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Customers.Create'),(19,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Customers.Edit'),(20,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Customers.Delete'),(21,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Users.View'),(22,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Users.Create'),(23,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Users.Edit'),(24,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Users.Delete'),(25,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Roles.View'),(26,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Roles.Create'),(27,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Roles.Edit'),(28,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Roles.Delete'),(29,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Debts.View'),(30,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Debts.Create'),(31,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Debts.Edit'),(32,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Debts.Delete'),(33,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Plans.View'),(34,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Plans.Create'),(35,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Plans.Edit'),(36,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Plans.Delete'),(37,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Norms.View'),(38,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Norms.Create'),(39,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Norms.Edit'),(40,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Norms.Delete'),(41,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Shipments.View'),(42,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Shipments.Create'),(43,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Shipments.Edit'),(44,'56b09bb8-7e2d-42bc-b34d-f477fec9ce17','Permission','Permissions.Shipments.Delete');
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table shipments
-- 

/*!40000 ALTER TABLE `shipments` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table shipmentdetails
-- 

/*!40000 ALTER TABLE `shipmentdetails` DISABLE KEYS */;
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
INSERT INTO `users`(`Id`,`FullName`,`Address`,`IsActive`,`CreatedAt`,`UserName`,`NormalizedUserName`,`Email`,`NormalizedEmail`,`EmailConfirmed`,`PasswordHash`,`SecurityStamp`,`ConcurrencyStamp`,`PhoneNumber`,`PhoneNumberConfirmed`,`TwoFactorEnabled`,`LockoutEnd`,`LockoutEnabled`,`AccessFailedCount`) VALUES('e89e777c-547c-48a7-96a2-d320e2128e52','System Administrator','',1,'2026-04-16 21:48:38.243863','sitinhtutu6@gmail.com','SITINHTUTU6@GMAIL.COM','sitinhtutu6@gmail.com','SITINHTUTU6@GMAIL.COM',1,'AQAAAAIAAYagAAAAEIIsgyAKopZiL+VYRwRRtf/EqWA/ysryIVsb6COqo2XedxNNibD+2fPYsBT9jR0mTg==','36NYOHUWPWXTGFMOJYC5CSDLYXNGXBN4','75f04eec-90f5-49e7-980e-09959ecd75bc',NULL,0,0,NULL,1,0);
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
INSERT INTO `aspnetuserroles`(`UserId`,`RoleId`) VALUES('e89e777c-547c-48a7-96a2-d320e2128e52','56b09bb8-7e2d-42bc-b34d-f477fec9ce17');
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table warehouseitems
-- 

/*!40000 ALTER TABLE `warehouseitems` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table products
-- 

/*!40000 ALTER TABLE `products` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table purchaseorderdetails
-- 

/*!40000 ALTER TABLE `purchaseorderdetails` DISABLE KEYS */;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table stocktransactions
-- 

/*!40000 ALTER TABLE `stocktransactions` DISABLE KEYS */;
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
  `WOCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TargetQuantity` int NOT NULL,
  `FinishedQuantity` int NOT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `Status` int NOT NULL,
  `MachineId` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Notes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PlanColor` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductionIndex` int NOT NULL DEFAULT '0',
  `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  PRIMARY KEY (`Id`),
  KEY `IX_WorkOrder_OrderId` (`OrderId`),
  CONSTRAINT `FK_WorkOrder_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE
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


-- Dump completed on 2026-04-16 21:49:47
-- Total time: 0:0:0:0:265 (d:h:m:s:ms)
