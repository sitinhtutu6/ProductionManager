-- MySqlBackup.NET 2.6.5.0
-- Dump Time: 2026-02-25 22:21:27
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
-- Definition of auditlogs
-- 

DROP TABLE IF EXISTS `auditlogs`;
CREATE TABLE IF NOT EXISTS `auditlogs` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserName` varchar(255) DEFAULT NULL COMMENT 'Tên tài khoản người dùng thực hiện',
  `Action` varchar(50) NOT NULL COMMENT 'Hành động (Added, Modified, Deleted)',
  `TableName` varchar(100) NOT NULL COMMENT 'Tên bảng dữ liệu bị tác động',
  `RecordId` varchar(50) DEFAULT NULL COMMENT 'ID của dòng dữ liệu bị sửa/xóa',
  `OldValues` longtext COMMENT 'Dữ liệu cũ trước khi sửa (Định dạng JSON)',
  `NewValues` longtext COMMENT 'Dữ liệu mới sau khi sửa (Định dạng JSON)',
  `Timestamp` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Thời gian thực hiện thao tác',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table auditlogs
-- 

/*!40000 ALTER TABLE `auditlogs` DISABLE KEYS */;
INSERT INTO `auditlogs`(`Id`,`UserName`,`Action`,`TableName`,`RecordId`,`OldValues`,`NewValues`,`Timestamp`) VALUES(1,'SITINHTUTU6@GMAIL.COM','Added','WarehouseItem','-2147482647',NULL,'{\"Id\":-2147482647,\"Category\":\"Th\\u00E0nh Ph\\u1EA9m\",\"Code\":\"SP-0002\",\"CostPrice\":423000,\"ItemType\":\"Product\",\"Name\":\"Gh\\u1EBF KD1\",\"Note\":\"\\u0110\\u01B0\\u1EE3c t\\u1EA1o t\\u1EF1 \\u0111\\u1ED9ng t\\u1EEB Module S\\u1EA3n Ph\\u1EA9m\",\"StockQuantity\":0,\"Unit\":\"C\\u00E1i\"}','2026-02-20 23:05:13'),(2,'SITINHTUTU6@GMAIL.COM','Added','Product','-2147482647',NULL,'{\"Id\":-2147482647,\"Color\":\"Xanh\",\"DefaultPrice\":423000,\"HasVariants\":false,\"Length\":680,\"MaterialType\":\"Beech\",\"ProductCode\":\"SP-0002\",\"ProductName\":\"Gh\\u1EBF KD1\",\"Thick\":450,\"WarehouseItemId\":4,\"Width\":550}','2026-02-20 23:05:13'),(3,'SITINHTUTU6@GMAIL.COM','Modified','WarehouseItem','4','{\"Category\":\"Th\\u00E0nh Ph\\u1EA9m\",\"Code\":\"SP-0002\",\"CostPrice\":423000,\"ItemType\":\"Product\",\"Name\":\"Gh\\u1EBF KD1\",\"Note\":\"\\u0110\\u01B0\\u1EE3c t\\u1EA1o t\\u1EF1 \\u0111\\u1ED9ng t\\u1EEB Module S\\u1EA3n Ph\\u1EA9m\",\"StockQuantity\":0,\"Unit\":\"C\\u00E1i\"}','{\"Category\":\"Th\\u00E0nh Ph\\u1EA9m\",\"Code\":\"SP-0002\",\"CostPrice\":423000,\"ItemType\":\"Product\",\"LinkedProductId\":2,\"Name\":\"Gh\\u1EBF KD1\",\"Note\":\"\\u0110\\u01B0\\u1EE3c t\\u1EA1o t\\u1EF1 \\u0111\\u1ED9ng t\\u1EEB Module S\\u1EA3n Ph\\u1EA9m\",\"StockQuantity\":0,\"Unit\":\"C\\u00E1i\"}','2026-02-20 23:05:13'),(4,'SITINHTUTU6@GMAIL.COM','Added','StockTransaction','-2147482647',NULL,'{\"Id\":-2147482647,\"CurrentStock\":7,\"DocumentCode\":\"PX-20260222-1524\",\"Note\":\"\",\"Price\":38000,\"Quantity\":5,\"Receiver\":\"R\\u00E1p\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-22T08:24:12.49Z\",\"Type\":\"Export\",\"WarehouseItemId\":1}','2026-02-22 15:24:13'),(5,'SITINHTUTU6@GMAIL.COM','Modified','WarehouseItem','1','{\"StockQuantity\":12}','{\"StockQuantity\":7}','2026-02-22 15:24:13'),(6,'SITINHTUTU6@GMAIL.COM','Added','StockTransaction','-2147482647',NULL,'{\"Id\":-2147482647,\"CurrentStock\":7,\"DocumentCode\":\"PX-20260222-1524\",\"Note\":\"\",\"Price\":38000,\"Quantity\":5,\"Reason\":1,\"Receiver\":\"R\\u00E1p\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-22T10:07:30.326Z\",\"Type\":\"Export\",\"WarehouseItemId\":1}','2026-02-22 17:07:30'),(7,'SITINHTUTU6@GMAIL.COM','Deleted','StockTransaction','6','{\"Id\":6,\"CurrentStock\":7,\"DocumentCode\":\"PX-20260222-1524\",\"Note\":\"\",\"Price\":38000.00,\"Quantity\":5,\"Receiver\":\"R\\u00E1p\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-22T08:24:12.49\",\"Type\":\"Export\",\"WarehouseItemId\":1}',NULL,'2026-02-22 17:07:31'),(8,'SITINHTUTU6@GMAIL.COM','Added','StockTransaction','-2147482647',NULL,'{\"Id\":-2147482647,\"CurrentStock\":7,\"DocumentCode\":\"PX-20260217-2241\",\"Note\":\"\",\"Price\":38000,\"Quantity\":5,\"Reason\":1,\"Receiver\":\"R\\u00E1p\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-22T10:12:45.777Z\",\"Type\":\"Export\",\"WarehouseItemId\":1}','2026-02-22 17:12:46'),(9,'SITINHTUTU6@GMAIL.COM','Deleted','StockTransaction','5','{\"Id\":5,\"CurrentStock\":12,\"DocumentCode\":\"PX-20260217-2241\",\"Note\":\"\",\"Price\":38000.00,\"Quantity\":5,\"Receiver\":\"R\\u00E1p\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-17T15:41:57.041\",\"Type\":\"Export\",\"WarehouseItemId\":1}',NULL,'2026-02-22 17:12:46'),(10,'SITINHTUTU6@GMAIL.COM','Added','StockTransaction','-2147482647',NULL,'{\"Id\":-2147482647,\"CurrentStock\":2,\"DocumentCode\":\"PX-20260222-1731\",\"Note\":\"\",\"Price\":38000,\"Quantity\":5,\"Reason\":1,\"Receiver\":\"R\\u00E1p\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-22T10:32:05.278Z\",\"Type\":\"Export\",\"WarehouseItemId\":1}','2026-02-22 17:32:05'),(11,'SITINHTUTU6@GMAIL.COM','Modified','WarehouseItem','1','{\"StockQuantity\":7}','{\"StockQuantity\":2}','2026-02-22 17:32:05'),(12,'SITINHTUTU6@GMAIL.COM','Added','StockTransaction','-2147482647',NULL,'{\"Id\":-2147482647,\"CurrentStock\":-8,\"DocumentCode\":\"PX-20260223-1631\",\"Note\":\"\",\"OrderId\":1,\"Price\":38000,\"Quantity\":10,\"Reason\":1,\"Receiver\":\"T\\u1EA1o d\\u00E1ng\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-23T09:32:31.169Z\",\"Type\":\"Export\",\"WarehouseItemId\":1}','2026-02-23 16:32:31'),(13,'SITINHTUTU6@GMAIL.COM','Modified','WarehouseItem','1','{\"StockQuantity\":2}','{\"StockQuantity\":-8}','2026-02-23 16:32:31'),(14,'SITINHTUTU6@GMAIL.COM','Added','StockTransaction','-2147482647',NULL,'{\"Id\":-2147482647,\"CurrentStock\":-8,\"DocumentCode\":\"PX-20260223-1631\",\"Note\":\"\",\"OrderId\":1,\"Price\":38000,\"Quantity\":10,\"Reason\":1,\"Receiver\":\"T\\u1EA1o d\\u00E1ng\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-23T09:38:28.093Z\",\"Type\":\"Export\",\"WarehouseItemId\":1}','2026-02-23 16:38:28'),(15,'SITINHTUTU6@GMAIL.COM','Deleted','StockTransaction','10','{\"Id\":10,\"CurrentStock\":-8,\"DocumentCode\":\"PX-20260223-1631\",\"Note\":\"\",\"OrderId\":1,\"Price\":38000.00,\"Quantity\":10,\"Reason\":1,\"Receiver\":\"T\\u1EA1o d\\u00E1ng\",\"Staff\":\"SITINHTUTU6@GMAIL.COM\",\"TransactionDate\":\"2026-02-23T09:32:31.169\",\"Type\":\"Export\",\"WarehouseItemId\":1}',NULL,'2026-02-23 16:38:28'),(16,'SITINHTUTU6@GMAIL.COM','Added','ProductDetail','-2147482647',NULL,'{\"Id\":-2147482647,\"DetailCode\":\"CT-260223-0579\",\"Length\":0,\"Price\":0,\"ProductId\":2,\"Quantity\":1,\"Thick\":0,\"VariantName\":\"Ch\\u00E2n tr\\u01B0\\u1EDBc gh\\u1EBF KT3\",\"Width\":0}','2026-02-23 19:08:52'),(17,'SITINHTUTU6@GMAIL.COM','Modified','ProductDetail','1','{\"DetailCode\":\"CT-260223-0579\",\"Length\":0,\"Price\":0.00,\"ProductId\":2,\"Quantity\":1,\"Thick\":0,\"VariantName\":\"Ch\\u00E2n tr\\u01B0\\u1EDBc gh\\u1EBF KT3\",\"Width\":0}','{\"DetailCode\":\"CT-260223-0579\",\"Length\":550,\"Price\":0,\"ProductId\":2,\"Quantity\":1,\"Thick\":30,\"VariantName\":\"Ch\\u00E2n tr\\u01B0\\u1EDBc gh\\u1EBF KT3\",\"Width\":30}','2026-02-23 19:09:02'),(18,'SITINHTUTU6@GMAIL.COM','Modified','Product','2','{\"Color\":\"Xanh\",\"DefaultPrice\":423000.00,\"HasVariants\":false,\"Length\":680,\"MaterialType\":\"Beech\",\"ProductCode\":\"SP-0002\",\"ProductName\":\"Gh\\u1EBF KD1\",\"Thick\":450,\"WarehouseItemId\":4,\"Width\":550}','{\"Color\":\"Xanh\",\"DefaultPrice\":423000.00,\"HasVariants\":false,\"Length\":680,\"MaterialType\":\"Beech\",\"ProductCode\":\"SP-0002\",\"ProductName\":\"Gh\\u1EBF KD1\",\"Thick\":450,\"WarehouseItemId\":4,\"Width\":550}','2026-02-23 19:09:05'),(19,'SITINHTUTU6@GMAIL.COM','Added','ProductDocument','-2147482647',NULL,'{\"Id\":-2147482647,\"CreatedDate\":\"2026-02-25T20:14:10.0822478+07:00\",\"FileExtension\":\".xlsx\",\"FilePath\":\"sanpham/2026/dce92e75_KeHoachSX_030126 (2).xlsx\",\"IsActive\":true,\"Note\":\"Test ch\\u1EE9c n\\u0103ng qu\\u1EA3n l\\u00FD h\\u1ED3 s\\u01A1\",\"ProductId\":2,\"Title\":\"File test\"}','2026-02-25 20:14:10');
/*!40000 ALTER TABLE `auditlogs` ENABLE KEYS */;

-- 
-- Definition of companyconfigs
-- 

DROP TABLE IF EXISTS `companyconfigs`;
CREATE TABLE IF NOT EXISTS `companyconfigs` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CompanyName` varchar(255) NOT NULL,
  `Address` varchar(500) DEFAULT NULL,
  `TaxCode` varchar(50) DEFAULT NULL,
  `Phone` varchar(50) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table companyconfigs
-- 

/*!40000 ALTER TABLE `companyconfigs` DISABLE KEYS */;
INSERT INTO `companyconfigs`(`Id`,`CompanyName`,`Address`,`TaxCode`,`Phone`,`Email`) VALUES(1,'CÔNG TY TNHH SẢN XUẤT DEMO','KCN VSIP, TP. Thuận An, Bình Dương','0312345678','0909 123 456','contact@company.com');
/*!40000 ALTER TABLE `companyconfigs` ENABLE KEYS */;

-- 
-- Definition of costcategories
-- 

DROP TABLE IF EXISTS `costcategories`;
CREATE TABLE IF NOT EXISTS `costcategories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL COMMENT 'Tên loại chi phí hiển thị cho kế toán chọn',
  `Section` int NOT NULL COMMENT 'Mỏ neo P&L: 0=Exclude, 1=Salary, 2=Utilities, 3=Logistics, 4=Office, 5=Marketing, 99=Other',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1' COMMENT '1 = Đang sử dụng, 0 = Đã ẩn',
  `Description` text COMMENT 'Mô tả thêm (nếu có)',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table costcategories
-- 

/*!40000 ALTER TABLE `costcategories` DISABLE KEYS */;
INSERT INTO `costcategories`(`Id`,`Name`,`Section`,`IsActive`,`Description`) VALUES(1,'Lương & Phụ cấp nhân viên',1,1,'Tiền lương cơ bản, tăng ca, phụ cấp'),(2,'Bảo hiểm Xã hội (BHXH)',1,1,'Các khoản trích nộp bảo hiểm'),(3,'Chi phí Điện',2,1,'Tiền điện sinh hoạt/sản xuất hàng tháng'),(4,'Chi phí Nước & Rác',2,1,'Tiền nước, phí vệ sinh rác thải'),(5,'Cước Internet & Viễn thông',2,1,'Mạng Internet, tiền điện thoại công ty'),(6,'Xăng xe & Cước vận chuyển',3,1,'Tiền xăng, phí cầu đường, thuê xe ngoài'),(7,'Văn phòng phẩm',4,1,'Giấy in, bút, mực, kẹp tài liệu...'),(8,'Mua sắm công cụ dụng cụ',4,1,'Các vật dụng nhỏ dùng cho văn phòng'),(9,'Quảng cáo Facebook / Google',5,1,'Chi phí chạy Ads'),(10,'Tiếp khách & Ngoại giao',99,1,'Chi phí ăn uống tiếp khách, biếu tặng'),(11,'Chi phí phát sinh khác',99,1,'Các khoản OPEX lặt vặt khác chưa phân loại');
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
INSERT INTO `customers`(`Id`,`CustomerCode`,`CompanyName`,`Type`,`TaxCode`,`BankAccount`,`BankName`,`ContactPerson`,`PhoneNumber`,`Email`,`Address`,`Notes`) VALUES(1,'KH2602-1661','Vạn Phúc',1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);
/*!40000 ALTER TABLE `customers` ENABLE KEYS */;

-- 
-- Definition of debtadjustments
-- 

DROP TABLE IF EXISTS `debtadjustments`;
CREATE TABLE IF NOT EXISTS `debtadjustments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Date` datetime(6) NOT NULL,
  `Type` int NOT NULL COMMENT '1: Khách hàng (Phải thu), 2: NCC/Vận tải (Phải trả)',
  `PartnerName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Amount` decimal(18,2) NOT NULL,
  `Note` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `CreatedBy` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table debtadjustments
-- 

/*!40000 ALTER TABLE `debtadjustments` DISABLE KEYS */;
/*!40000 ALTER TABLE `debtadjustments` ENABLE KEYS */;

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
  `ShipmentCode` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `CustomerId` int NOT NULL,
  `CreatedDate` datetime NOT NULL,
  `ETD` datetime DEFAULT NULL,
  `ETA` datetime DEFAULT NULL,
  `BookingNumber` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `VesselName` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ContainerNumber` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `SealNumber` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `BLNumber` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CurrentStep` int NOT NULL DEFAULT '1',
  `ProgressPercent` int NOT NULL DEFAULT '0',
  `TransportType` int NOT NULL DEFAULT '0' COMMENT 'Loại hình: 0 = Container, 1 = Truck (Xe tải)',
  `CarrierName` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Tên nhà xe / Hãng tàu',
  `LicensePlate` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Biển số xe',
  `ShippingCost` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT 'Chi phí vận chuyển',
  `PaymentStatus` int NOT NULL DEFAULT '0' COMMENT 'Thanh toán: 0 = Unpaid (Ghi nợ), 1 = Paid (Đã trả)',
  `Status` int NOT NULL DEFAULT '0' COMMENT 'Trạng thái chuyến: 0=Planning, 1=Loading, 2=Shipped, 3=Completed',
  PRIMARY KEY (`Id`),
  KEY `FK_ExportShipments_Customers` (`CustomerId`),
  CONSTRAINT `FK_ExportShipments_Customers` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table exportshipments
-- 

/*!40000 ALTER TABLE `exportshipments` DISABLE KEYS */;
INSERT INTO `exportshipments`(`Id`,`ShipmentCode`,`CustomerId`,`CreatedDate`,`ETD`,`ETA`,`BookingNumber`,`VesselName`,`ContainerNumber`,`SealNumber`,`BLNumber`,`CurrentStep`,`ProgressPercent`,`TransportType`,`CarrierName`,`LicensePlate`,`ShippingCost`,`PaymentStatus`,`Status`) VALUES(1,'PX-20251230-1',1,'2026-02-17 12:04:36','2026-02-17 00:00:00',NULL,'SGN13554',NULL,NULL,NULL,NULL,2,61,0,NULL,NULL,0.00,0,0),(2,'PX-20251230-2',1,'2026-02-19 22:15:17',NULL,NULL,NULL,NULL,NULL,NULL,NULL,1,0,1,'Công Thành','51G-531.89',65000.00,0,1);
/*!40000 ALTER TABLE `exportshipments` ENABLE KEYS */;

-- 
-- Definition of exportdocuments
-- 

DROP TABLE IF EXISTS `exportdocuments`;
CREATE TABLE IF NOT EXISTS `exportdocuments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ExportShipmentId` int NOT NULL,
  `Step` int NOT NULL,
  `FileName` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `FilePath` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `UploadDate` datetime NOT NULL,
  `Note` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`Id`),
  KEY `FK_Documents_Shipment` (`ExportShipmentId`),
  CONSTRAINT `FK_Documents_Shipment` FOREIGN KEY (`ExportShipmentId`) REFERENCES `exportshipments` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table exportdocuments
-- 

/*!40000 ALTER TABLE `exportdocuments` DISABLE KEYS */;
/*!40000 ALTER TABLE `exportdocuments` ENABLE KEYS */;

-- 
-- Definition of exportshipmentorders
-- 

DROP TABLE IF EXISTS `exportshipmentorders`;
CREATE TABLE IF NOT EXISTS `exportshipmentorders` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ExportShipmentId` int NOT NULL,
  `OrderId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_ShipmentOrders_Shipment` (`ExportShipmentId`),
  KEY `FK_ShipmentOrders_Order` (`OrderId`),
  CONSTRAINT `FK_ShipmentOrders_Order` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ShipmentOrders_Shipment` FOREIGN KEY (`ExportShipmentId`) REFERENCES `exportshipments` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table exportshipmentorders
-- 

/*!40000 ALTER TABLE `exportshipmentorders` DISABLE KEYS */;
INSERT INTO `exportshipmentorders`(`Id`,`ExportShipmentId`,`OrderId`) VALUES(8,2,1);
/*!40000 ALTER TABLE `exportshipmentorders` ENABLE KEYS */;

-- 
-- Definition of financialperiods
-- 

DROP TABLE IF EXISTS `financialperiods`;
CREATE TABLE IF NOT EXISTS `financialperiods` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Month` int NOT NULL,
  `Year` int NOT NULL,
  `IsClosed` tinyint(1) NOT NULL DEFAULT '0',
  `ClosedAt` datetime(6) DEFAULT NULL,
  `ClosedBy` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table financialperiods
-- 

/*!40000 ALTER TABLE `financialperiods` DISABLE KEYS */;
/*!40000 ALTER TABLE `financialperiods` ENABLE KEYS */;

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
  `IsLocked` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table materialimports
-- 

/*!40000 ALTER TABLE `materialimports` DISABLE KEYS */;
INSERT INTO `materialimports`(`Id`,`ImportCode`,`ImportDate`,`SupplierId`,`SupplierName`,`TotalAmount`,`PaidAmount`,`PaymentStatus`,`IsLocked`) VALUES(1,'PN-20260212-2346','2026-02-12 23:46:00.000000',0,'Nhuận Phát',380000.00000000000000000000000,0.0000000000000000000000000000,0,0),(2,'PX-20260214-1524','2026-02-14 15:24:00.000000',0,'Nhuận Phát',380000.00000000000000000000000,0.0000000000000000000000000000,0,0),(3,'PN-20260217-2241','2026-02-17 15:41:37.583000',0,'Nhuận Phát',0.0000000000000000000000000000,0.0000000000000000000000000000,0,0);
/*!40000 ALTER TABLE `materialimports` ENABLE KEYS */;

-- 
-- Definition of importpricehistories
-- 

DROP TABLE IF EXISTS `importpricehistories`;
CREATE TABLE IF NOT EXISTS `importpricehistories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MaterialImportId` int NOT NULL,
  `OldTotalAmount` decimal(18,2) NOT NULL,
  `NewTotalAmount` decimal(18,2) NOT NULL,
  `Reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `ChangedAt` datetime(6) NOT NULL,
  `ChangedBy` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ImportPriceHistories_MaterialImportId` (`MaterialImportId`),
  CONSTRAINT `FK_ImportPriceHistories_MaterialImports` FOREIGN KEY (`MaterialImportId`) REFERENCES `materialimports` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table importpricehistories
-- 

/*!40000 ALTER TABLE `importpricehistories` DISABLE KEYS */;
/*!40000 ALTER TABLE `importpricehistories` ENABLE KEYS */;

-- 
-- Definition of orders
-- 

DROP TABLE IF EXISTS `orders`;
CREATE TABLE IF NOT EXISTS `orders` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
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
  `InvoiceFile` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PaymentStatus` int NOT NULL,
  `InvoiceNumber` varchar(50) DEFAULT NULL COMMENT 'Số hóa đơn đỏ',
  `InvoiceDate` datetime DEFAULT NULL COMMENT 'Ngày xuất hóa đơn',
  `PaidAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT 'Số tiền khách đã trả',
  `PaperOrderCode` varchar(100) DEFAULT NULL COMMENT 'Mã đơn hàng trên giấy/chứng từ gốc',
  `TotalCOGS` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT 'Tổng giá vốn hàng bán của đơn hàng (Tiền vật tư tiêu hao)',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Orders_OrderCode` (`OrderCode`),
  KEY `IX_Orders_CustomerId` (`CustomerId`),
  CONSTRAINT `FK_Orders_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table orders
-- 

/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
INSERT INTO `orders`(`Id`,`OrderCode`,`CustomerId`,`OrderDate`,`DeliveryDeadline`,`TotalAmount`,`DepositAmount`,`Status`,`IsLocked`,`Notes`,`DesignFile`,`ProductionIndex`,`ProductionStartDate`,`ProductionEndDate`,`PlanColor`,`InvoiceFile`,`PaymentStatus`,`InvoiceNumber`,`InvoiceDate`,`PaidAmount`,`PaperOrderCode`,`TotalCOGS`) VALUES(1,'DH-001',1,'2026-02-11 19:05:39.269585','2026-02-17 00:00:00.000000',39420000.00,0.00,2,0,NULL,NULL,9999,NULL,NULL,NULL,NULL,0,NULL,NULL,0.00,'CT-001',0.00);
/*!40000 ALTER TABLE `orders` ENABLE KEYS */;

-- 
-- Definition of cashentries
-- 

DROP TABLE IF EXISTS `cashentries`;
CREATE TABLE IF NOT EXISTS `cashentries` (
  `Id` int NOT NULL AUTO_INCREMENT,
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
  `PaperVoucherNumber` longtext,
  `CostCategoryId` int DEFAULT NULL COMMENT 'Khóa ngoại liên kết tới bảng CostCategories',
  `ForMonth` int DEFAULT NULL COMMENT 'Chi phí thuộc tháng',
  `ForYear` int DEFAULT NULL COMMENT 'Chi phí thuộc năm',
  `AllocatedMonths` int NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  KEY `IX_CashEntries_MaterialImportId` (`MaterialImportId`),
  KEY `IX_CashEntries_OrderId` (`OrderId`),
  KEY `IX_CashEntries_ParentId` (`ParentId`),
  KEY `FK_CashEntries_CostCategories` (`CostCategoryId`),
  CONSTRAINT `FK_CashEntries_CashEntries_ParentId` FOREIGN KEY (`ParentId`) REFERENCES `cashentries` (`Id`),
  CONSTRAINT `FK_CashEntries_CostCategories` FOREIGN KEY (`CostCategoryId`) REFERENCES `costcategories` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_CashEntries_MaterialImports_MaterialImportId` FOREIGN KEY (`MaterialImportId`) REFERENCES `materialimports` (`Id`),
  CONSTRAINT `FK_CashEntries_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table cashentries
-- 

/*!40000 ALTER TABLE `cashentries` DISABLE KEYS */;
INSERT INTO `cashentries`(`Id`,`TransactionDate`,`ReportDate`,`VoucherCode`,`Type`,`Method`,`Category`,`Amount`,`Description`,`TargetName`,`OrderId`,`ParentId`,`CreatedBy`,`CreatedAt`,`MaterialImportId`,`PaperVoucherNumber`,`CostCategoryId`,`ForMonth`,`ForYear`,`AllocatedMonths`) VALUES(1,'2026-02-12 00:00:00.000000','2026-02-12 00:19:17.347435','PT2602-001',1,1,1,10000000.00,NULL,'aa',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-12 00:19:17.347435',NULL,'PC-001',NULL,NULL,NULL,1),(2,'2026-01-02 00:00:00.000000','2026-01-02 00:00:00.000000','AUTO-0003',2,1,1,36000.00,'Thanh toán tiền mua dây curoa','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647287',NULL,'PC03',NULL,NULL,NULL,1),(3,'2026-01-04 00:00:00.000000','2026-01-04 00:00:00.000000','AUTO-0004',2,1,1,735000.00,'Chi tiền đồ cúng (16.11)','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647477',NULL,'PC04',NULL,NULL,NULL,1),(4,'2026-01-05 00:00:00.000000','2026-01-05 00:00:00.000000','AUTO-0005',2,1,1,40000.00,'Thanh toán tiền ship dao','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647496',NULL,'PC05',NULL,NULL,NULL,1),(5,'2026-01-05 00:00:00.000000','2026-01-05 00:00:00.000000','AUTO-0006',2,1,1,320000.00,'Chi tiền cơm gia công','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647518',NULL,'PC06',NULL,NULL,NULL,1),(6,'2026-01-05 00:00:00.000000','2026-01-05 00:00:00.000000','AUTO-0007',2,1,1,11970000.00,'Thanh toán tiền gia công - PHAN CHI TAM','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647534',NULL,'PC07',NULL,NULL,NULL,1),(7,'2026-01-06 00:00:00.000000','2026-01-06 00:00:00.000000','AUTO-0008',2,1,1,1200000.00,'Thanh toán tiền xe chở gỗ Tavico','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647552',NULL,'PC08',NULL,NULL,NULL,1),(8,'2026-01-06 00:00:00.000000','2026-01-06 00:00:00.000000','AUTO-0009',2,1,1,4739750.00,'Thanh toán tiền uốn cong Thiên Phúc','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647570',NULL,'PC09',NULL,NULL,NULL,1),(9,'2026-01-07 00:00:00.000000','2026-01-07 00:00:00.000000','AUTO-0010',2,1,1,36000.00,'Chi mua dây curoa B71','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647586',NULL,'PC10',NULL,NULL,NULL,1),(10,'2026-01-07 00:00:00.000000','2026-01-07 00:00:00.000000','AUTO-0011',2,1,1,34000.00,'Chi mua dây curoa M37','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647602',NULL,'PC11',NULL,NULL,NULL,1),(11,'2026-01-08 00:00:00.000000','2026-01-08 00:00:00.000000','AUTO-0012',2,1,1,500000.00,'Chi thanh toán tiền xe - Đại Hoàng An','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647618',NULL,'PC12',NULL,NULL,NULL,1),(12,'2026-01-08 00:00:00.000000','2026-01-08 00:00:00.000000','AUTO-0013',2,1,1,120000.00,'Chi mua dao arden 112x10x30','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647633',NULL,'PC13',NULL,NULL,NULL,1),(13,'2026-01-08 00:00:00.000000','2026-01-08 00:00:00.000000','AUTO-0014',2,1,1,1150000.00,'Chi sửa máy nhám','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647648',NULL,'PC14',NULL,NULL,NULL,1),(14,'2026-01-09 00:00:00.000000','2026-01-09 00:00:00.000000','AUTO-0015',2,1,1,2009000.00,'Chi mua sò dao','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647664',NULL,'PC15',NULL,NULL,NULL,1),(15,'2026-01-09 00:00:00.000000','2026-01-09 00:00:00.000000','AUTO-0016',2,1,1,209840.00,'Chi thanh toán thuế HĐ 58 Nhuận Phát','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647683',NULL,'PC16',NULL,NULL,NULL,1),(16,'2026-01-09 00:00:00.000000','2026-01-09 00:00:00.000000','AUTO-0017',2,1,1,18253412.00,'Thanh toán tiền điện kỳ 3 tháng 12','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647699',NULL,'PC17',NULL,NULL,NULL,1),(17,'2026-01-09 00:00:00.000000','2026-01-09 00:00:00.000000','AUTO-0018',1,1,1,370000000.00,'Nhập quỹ tiềm mặt','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647714',NULL,'PT01',NULL,NULL,NULL,1),(18,'2026-01-09 00:00:00.000000','2026-01-09 00:00:00.000000','AUTO-0019',2,1,1,372000000.00,'Chi thanh toán lương','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647729',NULL,'PC18',NULL,NULL,NULL,1),(19,'2026-01-12 00:00:00.000000','2026-01-12 00:00:00.000000','AUTO-0020',2,1,1,250000.00,'Chi tiền cảm biến máy nhám','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647744',NULL,'PC19',NULL,NULL,NULL,1),(20,'2026-01-12 00:00:00.000000','2026-01-12 00:00:00.000000','AUTO-0021',2,1,1,170000.00,'Chi tiền cơm gia công','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647759',NULL,'PC20',NULL,NULL,NULL,1),(21,'2026-01-12 00:00:00.000000','2026-01-12 00:00:00.000000','AUTO-0022',2,1,1,3580000.00,'Chi sửa máy nhám thùng','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647774',NULL,'PC21',NULL,NULL,NULL,1),(22,'2026-01-13 00:00:00.000000','2026-01-13 00:00:00.000000','AUTO-0023',2,1,1,3719900.00,'Thanh toán gia công chị Hằng','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647789',NULL,'PC22',NULL,NULL,NULL,1),(23,'2026-01-13 00:00:00.000000','2026-01-13 00:00:00.000000','AUTO-0024',2,1,1,5340000.00,'Thanh toán lương gia công tháng 01 đợt 1','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647804',NULL,'PC23',NULL,NULL,NULL,1),(24,'2026-01-13 00:00:00.000000','2026-01-13 00:00:00.000000','AUTO-0025',2,1,1,3380000.00,'Thanh toán tiền thuê xe nâng + thay nhớt tháng 01','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647821',NULL,'PC24',NULL,NULL,NULL,1),(25,'2026-01-13 00:00:00.000000','2026-01-13 00:00:00.000000','AUTO-0026',2,1,1,323200.00,'Thanh toán tiền VAT hóa đơn 06 - AN TIẾN PHÁT','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647836',NULL,'PC25',NULL,NULL,NULL,1),(26,'2026-01-14 00:00:00.000000','2026-01-14 00:00:00.000000','AUTO-0027',2,1,1,2800000.00,'Thanh toán tiền xe ba gác','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647852',NULL,'PC26',NULL,NULL,NULL,1),(27,'2026-01-14 00:00:00.000000','2026-01-14 00:00:00.000000','AUTO-0028',2,1,1,150000.00,'Thanh toán tiền mua nước suối cho văn phòng','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647866',NULL,'PC27',NULL,NULL,NULL,1),(28,'2026-01-14 00:00:00.000000','2026-01-14 00:00:00.000000','AUTO-0029',2,1,1,26000.00,'Chi mua dây A37','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647881',NULL,'PC28',NULL,NULL,NULL,1),(29,'2026-01-15 00:00:00.000000','2026-01-15 00:00:00.000000','AUTO-0030',2,1,1,600000.00,'Thanh toán tiền mua nhớt','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647895',NULL,'PC29',NULL,NULL,NULL,1),(30,'2026-01-15 00:00:00.000000','2026-01-15 00:00:00.000000','AUTO-0031',2,1,1,15700803.00,'Thanh toán tiền điện kỳ 1 tháng 1','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647910',NULL,'PC30',NULL,NULL,NULL,1),(31,'2026-01-16 00:00:00.000000','2026-01-16 00:00:00.000000','AUTO-0032',2,1,1,400000.00,'Chuyển thanh toán tiền xe','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647926',NULL,'PC31',NULL,NULL,NULL,1),(32,'2026-01-16 00:00:00.000000','2026-01-16 00:00:00.000000','AUTO-0033',2,1,1,450000.00,'Tiền lương gia công tupi (ngày 14-15)','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647941',NULL,'PC32',NULL,NULL,NULL,1),(33,'2026-01-16 00:00:00.000000','2026-01-16 00:00:00.000000','AUTO-0034',2,1,1,2665000.00,'Chi mua chốt, súng, mk, ống hút bụi','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.647955',NULL,'PC33',NULL,NULL,NULL,1),(34,'2026-01-16 00:00:00.000000','2026-01-16 00:00:00.000000','AUTO-0035',2,1,1,250000.00,'Chi tiền xe ký mẫu qua MPĐ','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648019',NULL,'PC34',NULL,NULL,NULL,1),(35,'2026-01-16 00:00:00.000000','2026-01-16 00:00:00.000000','AUTO-0036',2,1,1,157000.00,'Lương gia công Tupi','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648037',NULL,'PC35',NULL,NULL,NULL,1),(36,'2026-01-17 00:00:00.000000','2026-01-17 00:00:00.000000','AUTO-0037',2,1,1,270000.00,'Lương gia công Tupi','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648053',NULL,'PC36',NULL,NULL,NULL,1),(37,'2026-01-17 00:00:00.000000','2026-01-17 00:00:00.000000','AUTO-0038',2,1,1,5109000.00,'Chi tiếp khách','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648068',NULL,'PC37',NULL,NULL,NULL,1),(38,'2026-01-17 00:00:00.000000','2026-01-17 00:00:00.000000','AUTO-0039',2,1,1,5000000.00,'Chi mua dao mộng dương','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648082',NULL,'PC38',NULL,NULL,NULL,1),(39,'2026-01-17 00:00:00.000000','2026-01-17 00:00:00.000000','AUTO-0040',2,1,1,2044000.00,'Chi mua ben hơi, van gạt,..','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648097',NULL,'PC39',NULL,NULL,NULL,1),(40,'2026-01-17 00:00:00.000000','2026-01-17 00:00:00.000000','AUTO-0041',2,1,1,60000000.00,'Thanh toán tiền thuê xưởng','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648112',NULL,'PC40',NULL,NULL,NULL,1),(41,'2026-01-17 00:00:00.000000','2026-01-17 00:00:00.000000','AUTO-0042',2,1,1,9996000.00,'Thanh toán tiền gia công - PHAN CHI TAM','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648127',NULL,'PC41',NULL,NULL,NULL,1),(42,'2026-01-19 00:00:00.000000','2026-01-19 00:00:00.000000','AUTO-0043',2,1,1,26369500.00,'Thanh toán nợ lương','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648142',NULL,'PC42',NULL,NULL,NULL,1),(43,'2026-01-19 00:00:00.000000','2026-01-19 00:00:00.000000','AUTO-0044',2,1,1,350000.00,'Thanh toán tiền xe chở gỗ - Việt Âu Mỹ','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648169',NULL,'PC43',NULL,NULL,NULL,1),(44,'2026-01-20 00:00:00.000000','2026-01-20 00:00:00.000000','AUTO-0045',2,1,1,320000.00,'Thanh toán tiền sửa máy nhám','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648185',NULL,'PC44',NULL,NULL,NULL,1),(45,'2026-01-20 00:00:00.000000','2026-01-20 00:00:00.000000','AUTO-0046',2,1,1,450000.00,'Chi mua đồ cúng (mùng 2.12)','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648200',NULL,'PC45',NULL,NULL,NULL,1),(46,'2026-01-20 00:00:00.000000','2026-01-20 00:00:00.000000','AUTO-0047',2,1,1,500000.00,'Chi mua dâu curoa 40x2200','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648214',NULL,'PC46',NULL,NULL,NULL,1),(47,'2026-01-20 00:00:00.000000','2026-01-20 00:00:00.000000','AUTO-0048',2,1,1,77000.00,'Chi mua băng keo cá nhân','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648230',NULL,'PC47',NULL,NULL,NULL,1),(48,'2026-01-21 00:00:00.000000','2026-01-21 00:00:00.000000','AUTO-0049',2,1,1,180000.00,'Chi tiền ship mẫu qua Đông Phương','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648245',NULL,'PC48',NULL,NULL,NULL,1),(49,'2026-01-21 00:00:00.000000','2026-01-21 00:00:00.000000','AUTO-0050',2,1,1,200000.00,'Chi tiền xe chở ván ép qua Minh Phương Đông','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648259',NULL,'PC49',NULL,NULL,NULL,1),(50,'2026-01-22 00:00:00.000000','2026-01-22 00:00:00.000000','AUTO-0051',2,1,1,65000.00,'Chi mua dao','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648273',NULL,'PC50',NULL,NULL,NULL,1),(51,'2026-01-22 00:00:00.000000','2026-01-22 00:00:00.000000','AUTO-0052',2,1,1,250000.00,'Chi tiền xe qua Mộc Chất lấy ván ép (ngày 17-01)','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648287',NULL,'PC51',NULL,NULL,NULL,1),(52,'2026-01-22 00:00:00.000000','2026-01-22 00:00:00.000000','AUTO-0053',2,1,1,500000.00,'Chi mua dầu','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648302',NULL,'PC52',NULL,NULL,NULL,1),(53,'2026-01-23 00:00:00.000000','2026-01-23 00:00:00.000000','AUTO-0054',2,1,1,60000.00,'Chi mua dao','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648317',NULL,'PC53',NULL,NULL,NULL,1),(54,'2026-01-23 00:00:00.000000','2026-01-23 00:00:00.000000','AUTO-0055',2,1,1,300000.00,'Chi tiền rác tháng 1','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648331',NULL,'PC54',NULL,NULL,NULL,1),(55,'2026-01-23 00:00:00.000000','2026-01-23 00:00:00.000000','AUTO-0056',2,1,1,130000.00,'Chi mua dao','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648345',NULL,'PC55',NULL,NULL,NULL,1),(56,'2026-01-24 00:00:00.000000','2026-01-24 00:00:00.000000','AUTO-0057',2,1,1,190000.00,'Chi tiền ship mẫu qua Đông Phương','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648360',NULL,'PC56',NULL,NULL,NULL,1),(57,'2026-01-24 00:00:00.000000','2026-01-24 00:00:00.000000','AUTO-0058',2,1,1,846340.00,'Thanh toán phí VAT hóa đơn Nhuận Phát','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648374',NULL,'PC57',NULL,NULL,NULL,1),(58,'2026-01-24 00:00:00.000000','2026-01-24 00:00:00.000000','AUTO-0059',2,1,1,1390000.00,'Chi thanh toán lương gia công Tupi (ngày 23-24/01)','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648389',NULL,'PC58',NULL,NULL,NULL,1),(59,'2026-01-24 00:00:00.000000','2026-01-24 00:00:00.000000','AUTO-0060',2,1,1,600000.00,'Chi thanh toán tiền cơm gia công','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648404',NULL,'PC59',NULL,NULL,NULL,1),(60,'2026-01-24 00:00:00.000000','2026-01-24 00:00:00.000000','AUTO-0061',2,1,1,350000.00,'Chi thanh toán lương gia công phản mài','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648418',NULL,'PC60',NULL,NULL,NULL,1),(61,'2026-01-24 00:00:00.000000','2026-01-24 00:00:00.000000','AUTO-0062',2,1,1,14641555.00,'Thanh toán tiền điện kỳ 2 tháng 1','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648433',NULL,'PC61',NULL,NULL,NULL,1),(62,'2026-01-24 00:00:00.000000','2026-01-24 00:00:00.000000','AUTO-0063',2,1,1,25212500.00,'Thanh toán lương gia công tháng 01 đợt 2','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648447',NULL,'PC62',NULL,NULL,NULL,1),(63,'2026-01-26 00:00:00.000000','2026-01-26 00:00:00.000000','AUTO-0064',2,1,1,260000.00,'Chi mua chốt','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648462',NULL,'PC63',NULL,NULL,NULL,1),(64,'2026-01-26 00:00:00.000000','2026-01-26 00:00:00.000000','AUTO-0065',2,1,1,1400000.00,'Phí xử lý nhám','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648477',NULL,'PC64',NULL,NULL,NULL,1),(65,'2026-01-27 00:00:00.000000','2026-01-27 00:00:00.000000','AUTO-0066',2,1,1,800000.00,'Thanh toán phí làm mẫu tiện CNC Đông Phương','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648491',NULL,'PC65',NULL,NULL,NULL,1),(66,'2026-01-27 00:00:00.000000','2026-01-27 00:00:00.000000','AUTO-0067',2,1,1,1230000.00,'Thanh toán phiếu sửa máy','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648507',NULL,'PC66',NULL,NULL,NULL,1),(67,'2026-01-28 00:00:00.000000','2026-01-28 00:00:00.000000','AUTO-0068',2,1,1,390000.00,'Chi mua chốt','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648521',NULL,'PC67',NULL,NULL,NULL,1),(68,'2026-01-28 00:00:00.000000','2026-01-28 00:00:00.000000','AUTO-0069',2,1,1,1000000.00,'Chi cho QC MPĐ','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648537',NULL,'PC68',NULL,NULL,NULL,1),(69,'2026-01-29 00:00:00.000000','2026-01-29 00:00:00.000000','AUTO-0070',2,1,1,2500000.00,'Chi cho PCCC','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648551',NULL,'PC69',NULL,NULL,NULL,1),(70,'2026-01-30 00:00:00.000000','2026-01-30 00:00:00.000000','AUTO-0071',2,1,1,600000.00,'Chi mua nhớt','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648566',NULL,'PC70',NULL,NULL,NULL,1),(71,'2026-01-30 00:00:00.000000','2026-01-30 00:00:00.000000','AUTO-0072',2,1,1,390000.00,'Chi mua vít 4x30','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648580',NULL,'PC71',NULL,NULL,NULL,1),(72,'2026-01-30 00:00:00.000000','2026-01-30 00:00:00.000000','AUTO-0073',2,1,1,600000.00,'Thanh toán tiền xe - Đại Hoàng An','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648594',NULL,'PC72',NULL,NULL,NULL,1),(73,'2026-01-31 00:00:00.000000','2026-01-31 00:00:00.000000','AUTO-0074',2,1,1,750000.00,'Chi tiềm cơm gia công','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648618',NULL,'PC73',NULL,NULL,NULL,1),(74,'2026-01-31 00:00:00.000000','2026-01-31 00:00:00.000000','AUTO-0075',2,1,1,495000.00,'Lương gia công tupi (ngày 31/01/2026)','Không',NULL,NULL,'SITINHTUTU6@GMAIL.COM','2026-02-13 19:16:58.648634',NULL,'PC74',NULL,NULL,NULL,1);
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
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table orderdetails
-- 

/*!40000 ALTER TABLE `orderdetails` DISABLE KEYS */;
INSERT INTO `orderdetails`(`Id`,`OrderId`,`ProductName`,`ProductId`,`Specifications`,`Quantity`,`Notes`,`UnitPrice`,`VatPercent`) VALUES(1,1,'Ghế KT3',1,'50x50x50',100,NULL,365000.00,8);
/*!40000 ALTER TABLE `orderdetails` ENABLE KEYS */;

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
INSERT INTO `roles`(`Id`,`Name`,`NormalizedName`,`ConcurrencyStamp`) VALUES('2e64378b-0691-4b18-a7de-ec48ce965a43','Manager','MANAGER',NULL),('6d5e39d6-6c41-4117-ad12-a722e67ae948','User','USER',NULL),('badbef91-d79d-47cb-aedd-ab9857f7e575','Admin','ADMIN','4999f4c2-41c9-49fa-8e41-4981e35d52d2');
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
INSERT INTO `aspnetroleclaims`(`Id`,`RoleId`,`ClaimType`,`ClaimValue`) VALUES(1,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Products.View'),(2,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Products.Create'),(3,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Products.Edit'),(4,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Products.Delete'),(5,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Orders.View'),(6,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Orders.Create'),(7,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Orders.Edit'),(8,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Orders.Delete'),(9,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Orders.Approve'),(10,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Documents.View'),(11,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Documents.Create'),(12,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Documents.Delete'),(13,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Warehouse.View'),(14,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Warehouse.Create'),(15,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Warehouse.Edit'),(16,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Warehouse.Delete'),(17,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Customers.View'),(18,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Customers.Create'),(19,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Customers.Edit'),(20,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Customers.Delete'),(21,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Users.View'),(22,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Users.Create'),(23,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Users.Edit'),(24,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Users.Delete'),(25,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Roles.View'),(26,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Roles.Create'),(27,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Roles.Edit'),(28,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Roles.Delete'),(29,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Debts.View'),(30,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Debts.Create'),(31,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Debts.Edit'),(32,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Debts.Delete'),(33,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Plans.View'),(34,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Plans.Create'),(35,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Plans.Edit'),(36,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Plans.Delete'),(37,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Norms.View'),(38,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Norms.Create'),(39,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Norms.Edit'),(40,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Norms.Delete'),(41,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Shipments.View'),(42,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Shipments.Create'),(43,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Shipments.Edit'),(44,'badbef91-d79d-47cb-aedd-ab9857f7e575','Permission','Permissions.Shipments.Delete');
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
  `ExportShipmentId` int DEFAULT NULL COMMENT 'Khóa ngoại trỏ về bảng ExportShipments (Chuyến xe)',
  `Type` int NOT NULL DEFAULT '0' COMMENT 'Phân loại phiếu xuất: 0 = Standard (Xuất bán bình thường), 1 = Warranty (Xuất bù/Bảo hành)',
  `ParentShipmentId` int DEFAULT NULL COMMENT 'ID phiếu xuất gốc (dùng để liên kết Phiếu nhập trả với Phiếu xuất bù)',
  PRIMARY KEY (`Id`),
  KEY `IX_Shipments_CustomerId` (`CustomerId`),
  KEY `IX_Shipments_OrderId` (`OrderId`),
  KEY `FK_Shipments_ExportShipments` (`ExportShipmentId`),
  KEY `FK_Shipments_ParentShipment` (`ParentShipmentId`),
  CONSTRAINT `FK_Shipments_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Shipments_ExportShipments` FOREIGN KEY (`ExportShipmentId`) REFERENCES `exportshipments` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Shipments_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`),
  CONSTRAINT `FK_Shipments_ParentShipment` FOREIGN KEY (`ParentShipmentId`) REFERENCES `shipments` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table shipments
-- 

/*!40000 ALTER TABLE `shipments` DISABLE KEYS */;
INSERT INTO `shipments`(`Id`,`ShipmentCode`,`ShipmentDate`,`CustomerId`,`OrderId`,`VehicleNumber`,`Notes`,`ExportShipmentId`,`Type`,`ParentShipmentId`) VALUES(1,'PX-20260211-4643','2026-02-11 00:00:00.000000',1,1,NULL,NULL,NULL,0,NULL),(2,'PX-20260213-7469','2026-02-13 00:00:00.000000',1,1,NULL,NULL,NULL,0,NULL),(3,'PX-20260219-1834','2026-02-19 00:00:00.000000',1,1,'51G-531.89',NULL,2,0,NULL),(5,'PX-20260220-4526','2026-02-20 00:00:00.000000',1,1,'51G-531.89','[XUẤT BÙ/BẢO HÀNH] ',2,1,NULL),(6,'PNT-20260220-4526','2026-02-20 00:00:00.000000',1,1,'51G-531.89','[TỰ ĐỘNG] Nhập hàng lỗi về kho từ yêu cầu xuất bù mã: PX-20260220-4526',2,2,NULL),(7,'PX-20260220-2836','2026-02-20 00:00:00.000000',1,1,NULL,'[XUẤT BÙ/BẢO HÀNH] ',NULL,1,NULL),(8,'PNT-20260220-7141','2026-02-20 00:00:00.000000',1,1,NULL,'[TỰ ĐỘNG] Nhập hàng lỗi về kho. Xuất bù theo phiếu gốc: PX-20260220-2836',NULL,2,7);
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
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table shipmentdetails
-- 

/*!40000 ALTER TABLE `shipmentdetails` DISABLE KEYS */;
INSERT INTO `shipmentdetails`(`Id`,`ShipmentId`,`OrderDetailId`,`QuantityShipped`) VALUES(1,1,1,30),(2,2,1,35),(3,3,1,20),(5,5,1,15),(6,6,1,-15),(7,7,1,5),(8,8,1,-5);
/*!40000 ALTER TABLE `shipmentdetails` ENABLE KEYS */;

-- 
-- Definition of steptrackers
-- 

DROP TABLE IF EXISTS `steptrackers`;
CREATE TABLE IF NOT EXISTS `steptrackers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ExportShipmentId` int NOT NULL,
  `Step` int NOT NULL,
  `Status` int NOT NULL DEFAULT '0',
  `CompletedDate` datetime DEFAULT NULL,
  `Note` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`Id`),
  KEY `FK_Trackers_Shipment` (`ExportShipmentId`),
  CONSTRAINT `FK_Trackers_Shipment` FOREIGN KEY (`ExportShipmentId`) REFERENCES `exportshipments` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 
-- Dumping data for table steptrackers
-- 

/*!40000 ALTER TABLE `steptrackers` DISABLE KEYS */;
INSERT INTO `steptrackers`(`Id`,`ExportShipmentId`,`Step`,`Status`,`CompletedDate`,`Note`) VALUES(1,1,1,2,'2026-02-17 12:22:48',NULL),(2,1,2,2,'2026-02-17 12:42:40',NULL),(3,1,3,1,NULL,NULL),(4,1,4,1,NULL,NULL),(5,1,5,1,NULL,NULL),(6,1,6,1,NULL,NULL),(7,1,7,1,NULL,NULL),(8,1,8,1,NULL,NULL),(9,1,9,1,NULL,NULL);
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
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table systemnotifications
-- 

/*!40000 ALTER TABLE `systemnotifications` DISABLE KEYS */;
INSERT INTO `systemnotifications`(`Id`,`Message`,`StartTime`,`EndTime`,`IsEnabled`) VALUES(1,'Thông báo: lịch nghỉ Tết 2026 từ ngày .... đến ngày ....','2026-02-15 19:31:48.336000','2026-02-22 19:31:48.336000',1);
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
INSERT INTO `users`(`Id`,`FullName`,`Address`,`IsActive`,`CreatedAt`,`UserName`,`NormalizedUserName`,`Email`,`NormalizedEmail`,`EmailConfirmed`,`PasswordHash`,`SecurityStamp`,`ConcurrencyStamp`,`PhoneNumber`,`PhoneNumberConfirmed`,`TwoFactorEnabled`,`LockoutEnd`,`LockoutEnabled`,`AccessFailedCount`) VALUES('64ae5362-d079-4735-9b39-3f6341b4befd','System Administrator','',1,'2026-02-07 15:43:21.712188','SITINHTUTU6@GMAIL.COM','SITINHTUTU6@GMAIL.COM','SITINHTUTU6@GMAIL.COM','SITINHTUTU6@GMAIL.COM',1,'AQAAAAIAAYagAAAAEBIb01ekhWt/jZNli6ZLYkprn55lcUFZcnCNeAuDM/T8viAzJc1E6R0CFq6p1zI0hA==','OVI3BG6T25X63FBFBQGLKTFGAAAPOHZE','9f3ae01e-6633-4c11-952c-c64b1902128d',NULL,0,0,NULL,1,0);
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
INSERT INTO `aspnetuserroles`(`UserId`,`RoleId`) VALUES('64ae5362-d079-4735-9b39-3f6341b4befd','badbef91-d79d-47cb-aedd-ab9857f7e575');
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
  `Name` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table warehousecategories
-- 

/*!40000 ALTER TABLE `warehousecategories` DISABLE KEYS */;
INSERT INTO `warehousecategories`(`Id`,`Name`) VALUES(2,'Bán thành phẩm'),(6,'Chưa phân loại'),(4,'Gỗ'),(5,'Màu'),(1,'Nguyên vật liệu'),(3,'Thành phẩm');
/*!40000 ALTER TABLE `warehousecategories` ENABLE KEYS */;

-- 
-- Definition of warehouseitems
-- 

DROP TABLE IF EXISTS `warehouseitems`;
CREATE TABLE IF NOT EXISTS `warehouseitems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Unit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StockQuantity` double NOT NULL,
  `CostPrice` decimal(18,2) NOT NULL,
  `Note` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Category` varchar(100) DEFAULT 'Chưa phân loại' COMMENT 'Phân loại vật tư',
  `Image` varchar(255) DEFAULT NULL COMMENT 'Đường dẫn ảnh vật tư',
  `ItemType` varchar(50) DEFAULT 'Material' COMMENT 'Material: Vật tư, Product: Thành phẩm, SemiProduct: Bán thành phẩm',
  `LinkedProductId` int DEFAULT NULL COMMENT 'ID của sản phẩm bên bảng Products',
  `CategoryId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_WarehouseItems_Code` (`Code`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table warehouseitems
-- 

/*!40000 ALTER TABLE `warehouseitems` DISABLE KEYS */;
INSERT INTO `warehouseitems`(`Id`,`Code`,`Name`,`Unit`,`StockQuantity`,`CostPrice`,`Note`,`Category`,`Image`,`ItemType`,`LinkedProductId`,`CategoryId`) VALUES(1,'VT-0001','Vít 4x30','Kg',-8,38000.00,NULL,'Chưa phân loại','5838a887-e627-4377-a378-ee2f6489b9fc_Gemini_Generated_Image_unwijkunwijkunwi.png','Material',NULL,1),(2,'VT-0002','Gỗ ASH','Cái',0,0.00,NULL,'Nguyên vật liệu',NULL,'Material',NULL,1),(3,'SP-0001','Ghế KT3','Cái',0,365000.00,NULL,'Thành Phẩm',NULL,'Product',1,3),(4,'SP-0002','Ghế KD1','Cái',0,423000.00,'Được tạo tự động từ Module Sản Phẩm','Thành Phẩm',NULL,'Product',2,3);
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
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table productboms
-- 

/*!40000 ALTER TABLE `productboms` DISABLE KEYS */;
INSERT INTO `productboms`(`Id`,`ProductId`,`MaterialId`,`ComponentName`,`Length`,`Width`,`Height`,`CutQuantity`,`Coefficient`,`Quantity`,`Note`) VALUES(1,1,2,'Chân ghế',50,50,500,1,1,0.00125,NULL);
/*!40000 ALTER TABLE `productboms` ENABLE KEYS */;

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
  KEY `FK_ProductDetails_WarehouseItems` (`WarehouseItemId`),
  CONSTRAINT `FK_ProductDetails_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ProductDetails_WarehouseItems` FOREIGN KEY (`WarehouseItemId`) REFERENCES `warehouseitems` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table productdetails
-- 

/*!40000 ALTER TABLE `productdetails` DISABLE KEYS */;
INSERT INTO `productdetails`(`Id`,`ProductId`,`DetailCode`,`VariantName`,`Color`,`MaterialType`,`Thick`,`Width`,`Length`,`Quantity`,`Price`,`Note`,`WarehouseItemId`) VALUES(1,2,'CT-260223-0579','Chân trước ghế KT3',NULL,NULL,30,30,550,1,0.00,NULL,NULL);
/*!40000 ALTER TABLE `productdetails` ENABLE KEYS */;

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
  KEY `FK_Products_WarehouseItems` (`WarehouseItemId`),
  CONSTRAINT `FK_Products_WarehouseItems` FOREIGN KEY (`WarehouseItemId`) REFERENCES `warehouseitems` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table products
-- 

/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products`(`Id`,`ProductCode`,`ProductName`,`Color`,`MaterialType`,`Unit`,`Thick`,`Width`,`Length`,`Note`,`HasVariants`,`DefaultPrice`,`ImagePath`,`WarehouseItemId`) VALUES(1,'SP-0001','Ghế KT3','Xanh','Beech','Cái',50,50,50,NULL,0,365000.00,'0e9cda1c-8509-4cce-a883-bc7b6d827f50_Gemini_Generated_Image_x7p4ikx7p4ikx7p4.png',3),(2,'SP-0002','Ghế KD1','Xanh','Beech',NULL,450,550,680,NULL,0,423000.00,NULL,4);
/*!40000 ALTER TABLE `products` ENABLE KEYS */;

-- 
-- Definition of productdocuments
-- 

DROP TABLE IF EXISTS `productdocuments`;
CREATE TABLE IF NOT EXISTS `productdocuments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `Title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Note` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `FilePath` text,
  `FileExtension` varchar(50) DEFAULT NULL,
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsActive` bit(1) NOT NULL DEFAULT b'1',
  PRIMARY KEY (`Id`),
  KEY `IX_ProductDocuments_ProductId` (`ProductId`),
  CONSTRAINT `FK_ProductDocuments_Products` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table productdocuments
-- 

/*!40000 ALTER TABLE `productdocuments` DISABLE KEYS */;
INSERT INTO `productdocuments`(`Id`,`ProductId`,`Title`,`Note`,`FilePath`,`FileExtension`,`CreatedDate`,`IsActive`) VALUES(1,2,'File test','Test chức năng quản lý hồ sơ','sanpham/2026/dce92e75_KeHoachSX_030126 (2).xlsx','.xlsx','2026-02-25 20:14:10',1);
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
  `Reason` int DEFAULT NULL COMMENT 'Lý do xuất kho: 1=Sản xuất/Bán hàng, 2=Trả NCC, 3=Hao hụt/Hư hỏng',
  PRIMARY KEY (`Id`),
  KEY `IX_StockTransactions_OrderId` (`OrderId`),
  KEY `IX_StockTransactions_WarehouseItemId` (`WarehouseItemId`),
  CONSTRAINT `FK_StockTransactions_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`),
  CONSTRAINT `FK_StockTransactions_WarehouseItems_WarehouseItemId` FOREIGN KEY (`WarehouseItemId`) REFERENCES `warehouseitems` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 
-- Dumping data for table stocktransactions
-- 

/*!40000 ALTER TABLE `stocktransactions` DISABLE KEYS */;
INSERT INTO `stocktransactions`(`Id`,`WarehouseItemId`,`TransactionDate`,`Type`,`Quantity`,`CurrentStock`,`Price`,`Note`,`DocumentCode`,`Receiver`,`Staff`,`OrderId`,`Reason`) VALUES(1,1,'2026-02-12 23:46:00.000000','Import',10,10,38000.00,'','PN-20260212-2346','Nhuận Phát','SITINHTUTU6@GMAIL.COM',NULL,NULL),(2,1,'2026-02-12 23:46:00.000000','Export',8,2,38000.00,'','PX-20260212-2346','Ráp','SITINHTUTU6@GMAIL.COM',NULL,1),(3,1,'2026-02-14 15:24:00.000000','Import',10,12,38000.00,'','PX-20260214-1524','Nhuận Phát','SITINHTUTU6@GMAIL.COM',NULL,NULL),(4,1,'2026-02-17 15:41:37.583000','Import',5,17,0.00,'','PN-20260217-2241','Nhuận Phát','SITINHTUTU6@GMAIL.COM',NULL,NULL),(7,1,'2026-02-22 10:07:30.326000','Export',5,7,38000.00,'','PX-20260222-1524','Ráp','SITINHTUTU6@GMAIL.COM',NULL,1),(8,1,'2026-02-22 10:12:45.777000','Export',5,7,38000.00,'','PX-20260217-2241','Ráp','SITINHTUTU6@GMAIL.COM',NULL,1),(9,1,'2026-02-22 10:32:05.278000','Export',5,2,38000.00,'','PX-20260222-1731','Ráp','SITINHTUTU6@GMAIL.COM',NULL,1),(11,1,'2026-02-23 09:38:28.093000','Export',10,-8,38000.00,'','PX-20260223-1631','Tạo dáng','SITINHTUTU6@GMAIL.COM',1,1);
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


/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;


-- Dump completed on 2026-02-25 22:21:27
-- Total time: 0:0:0:0:171 (d:h:m:s:ms)
