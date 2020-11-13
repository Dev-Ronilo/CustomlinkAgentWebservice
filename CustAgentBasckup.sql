/*
SQLyog Community v11.51 (32 bit)
MySQL - 8.0.21-commercial : Database - CustDetails
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`CustDetails` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `CustDetails`;

/*Table structure for table `CustAgentMonitoring` */

DROP TABLE IF EXISTS `CustAgentMonitoring`;

CREATE TABLE `CustAgentMonitoring` (
  `ZoneCode` int DEFAULT NULL,
  `BranchCode` int DEFAULT NULL,
  `LastRunningDate` datetime DEFAULT NULL,
  `LastNotRunningDate` datetime DEFAULT NULL,
  `NoOfCountsNotRunning` int DEFAULT NULL,
  `CustAppVersion` varchar(100) DEFAULT NULL,
  `DTCreated` datetime DEFAULT NULL,
  `Transtype` int DEFAULT NULL COMMENT '0 = Not Running 1 = Running',
  KEY `i_branchcode` (`BranchCode`),
  KEY `i_zonecode` (`ZoneCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Table structure for table `CustLinkHistory` */

DROP TABLE IF EXISTS `CustLinkHistory`;

CREATE TABLE `CustLinkHistory` (
  `branchCode` int DEFAULT NULL,
  `zoneCode` int DEFAULT NULL,
  `dateCreated` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Table structure for table `InstalledBranches` */

DROP TABLE IF EXISTS `InstalledBranches`;

CREATE TABLE `InstalledBranches` (
  `ID` int unsigned NOT NULL AUTO_INCREMENT,
  `ZoneCode` int unsigned NOT NULL,
  `ZoneName` varchar(10) NOT NULL,
  `BranchCode` varchar(3) NOT NULL,
  `BranchName` varchar(100) NOT NULL,
  `AreaCode` varchar(100) DEFAULT NULL,
  `AreaName` varchar(100) NOT NULL,
  `RegionName` varchar(100) NOT NULL,
  `RegionCode` varchar(100) DEFAULT NULL,
  `DateInstalled` datetime NOT NULL,
  `DTCreated` datetime DEFAULT NULL,
  `ConsecutiveCountsNotRunning` int DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `i_region` (`RegionName`) USING BTREE,
  KEY `i_area` (`AreaName`) USING BTREE,
  KEY `i_zonecode` (`ZoneCode`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
