-- phpMyAdmin SQL Dump
-- version 4.0.10.14
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Jul 08, 2018 at 06:18 AM
-- Server version: 5.1.69
-- PHP Version: 5.5.15

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `Stella_Ai`
--

-- --------------------------------------------------------

--
-- Table structure for table `Aiml_Server`
--

CREATE TABLE IF NOT EXISTS `Aiml_Server` (
  `user_id` int(255) NOT NULL AUTO_INCREMENT,
  `username` varchar(255) NOT NULL,
  `question` varchar(255) NOT NULL,
  `bot_response` varchar(255) NOT NULL,
  `is_readed` varchar(255) NOT NULL,
  `ip_address` varchar(255) NOT NULL,
  PRIMARY KEY (`user_id`)
) ENGINE=MyISAM  DEFAULT CHARSET=utf8 AUTO_INCREMENT=2 ;

--
-- Dumping data for table `Aiml_Server`
--

INSERT INTO `Aiml_Server` (`user_id`, `username`, `question`, `bot_response`, `is_readed`, `ip_address`) VALUES
(1, 'Tahir', '', '', '', '192.168.43.3');

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
