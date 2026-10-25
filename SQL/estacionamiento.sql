CREATE DATABASE  IF NOT EXISTS `estacionamiento` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `estacionamiento`;
-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: estacionamiento
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20261013210932_IdentityInicial','10.0.13');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetroleclaims`
--

DROP TABLE IF EXISTS `aspnetroleclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `aspnetroleclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` int NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetRoleClaims_RoleId` (`RoleId`),
  CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `aspnetroles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetroleclaims`
--

LOCK TABLES `aspnetroleclaims` WRITE;
/*!40000 ALTER TABLE `aspnetroleclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetroleclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetroles`
--

DROP TABLE IF EXISTS `aspnetroles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `aspnetroles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NormalizedName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetroles`
--

LOCK TABLES `aspnetroles` WRITE;
/*!40000 ALTER TABLE `aspnetroles` DISABLE KEYS */;
INSERT INTO `aspnetroles` VALUES (1,'Admin','ADMIN','ba7224c0-dd20-475c-983c-5336aa4a384f'),(2,'Operador','OPERADOR','44762b1b-9f86-4604-87d2-07527868e7c5');
/*!40000 ALTER TABLE `aspnetroles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetuserclaims`
--

DROP TABLE IF EXISTS `aspnetuserclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `aspnetuserclaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetUserClaims_UserId` (`UserId`),
  CONSTRAINT `FK_AspNetUserClaims_Staff_UserId` FOREIGN KEY (`UserId`) REFERENCES `staff` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetuserclaims`
--

LOCK TABLES `aspnetuserclaims` WRITE;
/*!40000 ALTER TABLE `aspnetuserclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetuserlogins`
--

DROP TABLE IF EXISTS `aspnetuserlogins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `aspnetuserlogins` (
  `LoginProvider` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderKey` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderDisplayName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `UserId` int NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`),
  KEY `IX_AspNetUserLogins_UserId` (`UserId`),
  CONSTRAINT `FK_AspNetUserLogins_Staff_UserId` FOREIGN KEY (`UserId`) REFERENCES `staff` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetuserlogins`
--

LOCK TABLES `aspnetuserlogins` WRITE;
/*!40000 ALTER TABLE `aspnetuserlogins` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserlogins` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetuserroles`
--

DROP TABLE IF EXISTS `aspnetuserroles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `aspnetuserroles` (
  `UserId` int NOT NULL,
  `RoleId` int NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IX_AspNetUserRoles_RoleId` (`RoleId`),
  CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `aspnetroles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AspNetUserRoles_Staff_UserId` FOREIGN KEY (`UserId`) REFERENCES `staff` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetuserroles`
--

LOCK TABLES `aspnetuserroles` WRITE;
/*!40000 ALTER TABLE `aspnetuserroles` DISABLE KEYS */;
INSERT INTO `aspnetuserroles` VALUES (1,1),(2,2),(3,2);
/*!40000 ALTER TABLE `aspnetuserroles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetusertokens`
--

DROP TABLE IF EXISTS `aspnetusertokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `aspnetusertokens` (
  `UserId` int NOT NULL,
  `LoginProvider` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`UserId`,`LoginProvider`,`Name`),
  CONSTRAINT `FK_AspNetUserTokens_Staff_UserId` FOREIGN KEY (`UserId`) REFERENCES `staff` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetusertokens`
--

LOCK TABLES `aspnetusertokens` WRITE;
/*!40000 ALTER TABLE `aspnetusertokens` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetusertokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cliente`
--

DROP TABLE IF EXISTS `cliente`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cliente` (
  `Patente` varchar(10) NOT NULL,
  `Nombre` varchar(45) NOT NULL,
  `Apellido` varchar(45) NOT NULL,
  `Telefono` varchar(45) NOT NULL,
  PRIMARY KEY (`Patente`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cliente`
--

LOCK TABLES `cliente` WRITE;
/*!40000 ALTER TABLE `cliente` DISABLE KEYS */;
INSERT INTO `cliente` VALUES ('TEST001','prueba','paginacion','2664000000'),('TEST0010','prueba10','paginacion','2664000007'),('TEST0011','prueba11','paginacion','2664000009'),('TEST002','prueba','paginacion','2664000001'),('TEST003','prueba3','paginacion','2664000002'),('TEST004','prueba4','paginacion','2664000003'),('TEST005','prueba5','paginacion','2664000004'),('TEST006','prueba6','paginacion','2664000005'),('TEST007','prueba7','paginacion','2664000006'),('TEST008','prueba8','paginacion','2664000007'),('TEST009','prueba9','paginacion','2664000008');
/*!40000 ALTER TABLE `cliente` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `estadia`
--

DROP TABLE IF EXISTS `estadia`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `estadia` (
  `ID_Estadia` int NOT NULL AUTO_INCREMENT,
  `ID_Plaza` int NOT NULL,
  `Patente` varchar(10) NOT NULL,
  `ID_Staff` int NOT NULL,
  `Ingreso` datetime NOT NULL,
  `Egreso` datetime DEFAULT NULL,
  `Ocupado` tinyint(1) NOT NULL DEFAULT '1',
  `PlazaActiva` int GENERATED ALWAYS AS (if((`Ocupado` = 1),`ID_Plaza`,NULL)) STORED,
  `PatenteActiva` varchar(10) GENERATED ALWAYS AS (if((`Ocupado` = 1),`Patente`,NULL)) STORED,
  PRIMARY KEY (`ID_Estadia`),
  UNIQUE KEY `UX_estadia_plaza_activa` (`PlazaActiva`),
  UNIQUE KEY `UX_estadia_patente_activa` (`PatenteActiva`),
  KEY `IX_estadia_plaza` (`ID_Plaza`,`Ocupado`),
  KEY `IX_estadia_patente` (`Patente`),
  KEY `FK_estadia_staff` (`ID_Staff`),
  CONSTRAINT `FK_estadia_cliente` FOREIGN KEY (`Patente`) REFERENCES `cliente` (`Patente`),
  CONSTRAINT `FK_estadia_plaza` FOREIGN KEY (`ID_Plaza`) REFERENCES `plaza` (`ID_Plaza`),
  CONSTRAINT `FK_estadia_staff` FOREIGN KEY (`ID_Staff`) REFERENCES `staff` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `estadia`
--

LOCK TABLES `estadia` WRITE;
/*!40000 ALTER TABLE `estadia` DISABLE KEYS */;
INSERT INTO `estadia` (`ID_Estadia`, `ID_Plaza`, `Patente`, `ID_Staff`, `Ingreso`, `Egreso`, `Ocupado`) VALUES (1,1,'TEST001',1,'2026-10-25 18:18:43','2026-10-25 18:19:24',0),(2,1,'TEST0010',1,'2026-10-25 18:24:43','2026-10-25 18:27:18',0),(3,2,'TEST005',1,'2026-10-25 18:26:44','2026-10-25 18:27:12',0),(4,3,'TEST006',1,'2026-10-25 18:27:52','2026-10-25 18:27:54',0),(5,1,'TEST0010',1,'2026-10-25 18:32:26','2026-10-25 18:35:11',0),(6,7,'TEST0011',1,'2026-10-25 18:32:44','2026-10-25 18:35:29',0),(7,9,'TEST002',1,'2026-10-25 18:32:53',NULL,1),(8,11,'TEST004',1,'2026-10-25 18:33:01','2026-10-25 18:35:17',0),(9,5,'TEST006',1,'2026-10-25 18:33:08','2026-10-25 18:35:06',0),(10,6,'TEST003',1,'2026-10-25 18:33:28',NULL,1),(11,4,'TEST008',1,'2026-10-25 18:33:42','2026-10-25 18:35:02',0),(12,2,'TEST007',1,'2026-10-25 18:34:00','2026-10-25 18:35:25',0),(13,10,'TEST005',1,'2026-10-25 18:34:29','2026-10-25 18:34:58',0),(14,3,'TEST009',1,'2026-10-25 18:34:46','2026-10-25 18:34:51',0),(15,7,'TEST0010',1,'2026-10-25 18:50:55','2026-10-25 18:51:11',0),(16,11,'TEST0011',1,'2026-10-25 18:52:32','2026-10-25 18:52:38',0);
/*!40000 ALTER TABLE `estadia` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pago`
--

DROP TABLE IF EXISTS `pago`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pago` (
  `ID_Pago` int NOT NULL AUTO_INCREMENT,
  `ID_Estadia` int NOT NULL,
  `FechaEmision` datetime NOT NULL,
  `PrecioXHoraAplicado` decimal(10,2) NOT NULL,
  `MontoTotal` decimal(10,2) NOT NULL,
  `ComprobanteUrl` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`ID_Pago`),
  UNIQUE KEY `UX_pago_estadia` (`ID_Estadia`),
  CONSTRAINT `pago_ibfk_1` FOREIGN KEY (`ID_Estadia`) REFERENCES `estadia` (`ID_Estadia`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pago`
--

LOCK TABLES `pago` WRITE;
/*!40000 ALTER TABLE `pago` DISABLE KEYS */;
INSERT INTO `pago` VALUES (1,1,'2026-10-25 18:19:24',1000.00,1000.00,NULL),(2,3,'2026-10-25 18:27:12',1000.00,1000.00,NULL),(3,2,'2026-10-25 18:27:18',1000.00,1000.00,NULL),(4,4,'2026-10-25 18:27:54',850.00,850.00,NULL),(5,14,'2026-10-25 18:34:51',850.00,850.00,NULL),(6,13,'2026-10-25 18:34:58',1000.00,1000.00,NULL),(7,11,'2026-10-25 18:35:02',850.00,850.00,NULL),(8,9,'2026-10-25 18:35:06',1000.00,1000.00,NULL),(9,5,'2026-10-25 18:35:11',1000.00,1000.00,NULL),(10,8,'2026-10-25 18:35:17',850.00,850.00,NULL),(11,12,'2026-10-25 18:35:25',1000.00,1000.00,NULL),(12,6,'2026-10-25 18:35:29',1000.00,1000.00,NULL),(13,15,'2026-10-25 18:51:11',1000.00,1000.00,NULL),(14,16,'2026-10-25 18:52:38',850.00,850.00,NULL);
/*!40000 ALTER TABLE `pago` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `plaza`
--

DROP TABLE IF EXISTS `plaza`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `plaza` (
  `ID_Plaza` int NOT NULL,
  `ID_TipoDePlaza` int NOT NULL,
  `Piso` int NOT NULL,
  `Sector` varchar(45) NOT NULL,
  PRIMARY KEY (`ID_Plaza`),
  KEY `ID_TipoDePlaza_idx` (`ID_TipoDePlaza`),
  CONSTRAINT `ID_TipoDePlaza` FOREIGN KEY (`ID_TipoDePlaza`) REFERENCES `tipodeplaza` (`ID_TipoDePlaza`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `plaza`
--

LOCK TABLES `plaza` WRITE;
/*!40000 ALTER TABLE `plaza` DISABLE KEYS */;
INSERT INTO `plaza` VALUES (1,1,1,'A'),(2,1,1,'B'),(3,2,1,'B'),(4,2,2,'B'),(5,1,1,'C'),(6,1,1,'D'),(7,1,1,'A'),(9,1,1,'A'),(10,1,1,'A'),(11,2,1,'B');
/*!40000 ALTER TABLE `plaza` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `staff`
--

DROP TABLE IF EXISTS `staff`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `staff` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `AvatarUrl` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `staff`
--

LOCK TABLES `staff` WRITE;
/*!40000 ALTER TABLE `staff` DISABLE KEYS */;
INSERT INTO `staff` VALUES (1,'/uploads/avatars/285a647ad1e3478ba02672d033b72e42.jpg','admin','ADMIN','admin@estacionamiento.local','ADMIN@ESTACIONAMIENTO.LOCAL',1,'AQAAAAIAAYagAAAAEI8oK+FCYkrWYZYQDMqXHurPAWJ82wO3yy0XSnfhO+yPUNCcyAFt0iFKJVWmjZ1BIQ==','BHRB22N2PFRFR6YCUP52QDAT5KY3QFMR','8ac39b40-b42f-4a15-b1cb-190404077ec2',NULL,0,0,NULL,1,0),(2,'/uploads/avatars/7383e72f30c44070a66db2e07751653a.jpg','Juan','JUAN','abregu058@gmail.com','ABREGU058@GMAIL.COM',1,'AQAAAAIAAYagAAAAEEgpa+iWNvLgZJx+F+ZxHRtVMvGMqu/8jH9A5seD/fdoz0f5r6GwzX2QRizuQqVKSQ==','Y6ZLNMSZGFKDKTPBV3ADW2LFIEOJO7SF','c9d0c132-95ee-4809-a262-4c9a5a7a6e15',NULL,0,0,NULL,1,0),(3,NULL,'Operador','OPERADOR','Operador@example.com','OPERADOR@EXAMPLE.COM',1,'AQAAAAIAAYagAAAAEPBnHslK2cLM5s2XLo+o6PPuPG5hwHj9AxK2u7x8m8m6FcURYrkUvtZ0B9cCrBtxNA==','DK3IEKCC7UPIETGLCHYW3TVZJSIMNGWJ','eac75d9a-7d6d-4a94-a9e5-9df4d6bb51c4',NULL,0,0,NULL,1,0);
/*!40000 ALTER TABLE `staff` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipodeplaza`
--

DROP TABLE IF EXISTS `tipodeplaza`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tipodeplaza` (
  `ID_TipoDePlaza` int NOT NULL,
  `Tipo` varchar(45) NOT NULL,
  `PrecioXHora` decimal(10,0) NOT NULL,
  PRIMARY KEY (`ID_TipoDePlaza`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipodeplaza`
--

LOCK TABLES `tipodeplaza` WRITE;
/*!40000 ALTER TABLE `tipodeplaza` DISABLE KEYS */;
INSERT INTO `tipodeplaza` VALUES (1,'Auto',1000),(2,'Moto',850);
/*!40000 ALTER TABLE `tipodeplaza` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-25 19:23:47
