



/*''*/
CREATE TABLE IF NOT EXISTS `users` (
  `id_user` bigint(20) NOT NULL AUTO_INCREMENT,
  `login` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `email` varchar(255) NOT NULL,
  `register_id` varchar(255) NOT NULL,
  `permission` int(1) NULL, /*'1-admin,2-superAdmin'*/
  PRIMARY KEY (id_user)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `user_lost_pass` (
  `id_user_lost_pass` bigint(20) NOT NULL AUTO_INCREMENT,
  `id_user` bigint(20) NOT NULL,
  `code` varchar(255) NOT NULL,
  `code_create_date` DATETIME NOT NULL,
  FOREIGN KEY (id_user) REFERENCES users(id_user) ON DELETE CASCADE ON UPDATE CASCADE,
  PRIMARY KEY (id_user_lost_pass)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `user_friend` (
  `id_user_friend` bigint(20) NOT NULL AUTO_INCREMENT,
  `id_user` bigint(20) NOT NULL,
  `id_friend` bigint(20) NOT NULL,
  FOREIGN KEY (id_user) REFERENCES users(id_user) ON DELETE CASCADE ON UPDATE CASCADE,
  FOREIGN KEY (id_friend) REFERENCES users(id_user) ON DELETE CASCADE ON UPDATE CASCADE,
  PRIMARY KEY (id_user_friend)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `user_ignored` (
  `id_ignored` bigint(20) NOT NULL AUTO_INCREMENT,
  `id_user` bigint(20) NOT NULL,
  `id_user_ignored` bigint(20) NOT NULL,
  FOREIGN KEY (id_user) REFERENCES users(id_user) ON DELETE CASCADE ON UPDATE CASCADE,
  FOREIGN KEY (id_user_ignored) REFERENCES users(id_user) ON DELETE CASCADE ON UPDATE CASCADE,
  PRIMARY KEY (id_ignored)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `user_channel` (
  `id_user_channel` bigint(20) NOT NULL AUTO_INCREMENT,
  `id_user` bigint(20) NOT NULL,
  `id_channel` bigint(20) NOT NULL,
  `join_date` DATETIME NOT NULL,
  FOREIGN KEY (id_user) REFERENCES users(id_user) ON UPDATE CASCADE,
  FOREIGN KEY (id_channel) REFERENCES channel(id_channel) ON DELETE CASCADE ON UPDATE CASCADE,
  PRIMARY KEY (id_user_channel)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `channel` (
  `id_channel` bigint(20) NOT NULL AUTO_INCREMENT,
  `id_user_founder` bigint(20) NOT NULL,
  `channel_name` varchar(255) NOT NULL,
  `enter_password` varchar(255) NOT NULL,
  `admin_password` varchar(255) NOT NULL,
  `max_users` int(2) NOT NULL,
  `create_date` DATETIME NOT NULL,
  `welcome_message` varchar(255) NOT NULL,
  FOREIGN KEY (id_user_founder) REFERENCES users(id_user) ON DELETE CASCADE ON UPDATE CASCADE,
  PRIMARY KEY (id_channel)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `user_bans` (
  `id_ban` bigint(20) NOT NULL AUTO_INCREMENT,
  `id_user` bigint(20) NOT NULL,
  `reason` varchar(255) NOT NULL,
  `end_ban` DATETIME NOT NULL,
  FOREIGN KEY (id_user) REFERENCES users(id_user) ON DELETE CASCADE ON UPDATE CASCADE,
  PRIMARY KEY (id_ban)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


/*''
CREATE TABLE IF NOT EXISTS `message_archivum` (
  `message_archivum` bigint(20) NOT NULL AUTO_INCREMENT,
  `id_user` bigint(20) NOT NULL,
  `message` varchar(255) NOT NULL,
  `message_date` DATETIME NOT NULL,
  FOREIGN KEY (id_user) REFERENCES users(id_user) ON DELETE CASCADE ON UPDATE CASCADE,
  PRIMARY KEY (message_archivum)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
*/