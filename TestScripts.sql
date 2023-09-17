create database fct;
alter database fct CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
use fct;
create table metaData (mkey INT, versionDate datetime, version int unsigned, createdTime datetime, modifiedTime datetime, primary key(mkey));
create table logs (id bigint not null AUTO_INCREMENT, dateUtc datetime(6) not null, message mediumtext null, level varchar(32) not null, exception mediumtext null, trace mediumtext null, logger varchar(255) not null, primary key(id));
create table users (id bigint not null, email varchar(128) null, username varchar(128) not null, password bigint unsigned not null, requiresNewPassword bit not null, salt int unsigned not null, createdTime datetime not null, modifiedTime datetime not null, primary key(id));
create table cardSet(id bigint not null AUTO_INCREMENT, userId bigint not null, setName varchar(128), createdTime datetime not null, modifiedTime datetime not null, primary key(id));
create table cards(id bigint not null AUTO_INCREMENT, setId bigint not null, frontValue text, backValue text, createdTime datetime not null, modifiedTime datetime not null, primary key(id));
insert into metaData values (0, '0001-01-01', 0, UTC_TIMESTAMP(), UTC_TIMESTAMP());