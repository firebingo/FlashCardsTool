create database fct;
alter database fct CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
use fct;
create table metaData (mkey INT, versionDate datetime, version int unsigned, createdTime datetime, modifiedTime datetime, primary key(mkey));
create table logs (id bigint not null AUTO_INCREMENT, dateUtc datetime(6) not null, message mediumtext null, level varchar(32) not null, exception mediumtext null, trace mediumtext null, logger varchar(255) not null, primary key(id));
create table users (id bigint not null, email varchar(128) null, username varchar(128) not null, password varchar(128) not null, requiresNewPassword bit not null, salt int unsigned not null, disabled bit not null, createdTime datetime not null, modifiedTime datetime not null, primary key(id));
create unique index idx_usersUserName on users (username);
create unique index idx_usersEmail on users (email);
create table cardSet(id bigint not null AUTO_INCREMENT, userId bigint not null, setName varchar(128) not null, createdTime datetime not null, modifiedTime datetime not null, primary key(id), constraint kf_cardSetUserId foreign key (userId) references users(id) on delete cascade);
create table card(id bigint not null AUTO_INCREMENT, setId bigint not null, frontValue text null, backValue text null, createdTime datetime not null, modifiedTime datetime not null, primary key(id), constraint kf_cardSetId foreign key (setId) references cardSet(id) on delete cascade);
insert into metaData values (0, '0001-01-01', 0, UTC_TIMESTAMP(), UTC_TIMESTAMP());

create table cardSetCollection(id bigint not null AUTO_INCREMENT, userId bigint not null, collectionName varchar(128) not null, createdTime datetime not null, modifiedTime datetime not null, primary key(id), constraint kf_collectionUserId foreign key (userId) references users(id) on delete cascade);
create table cardSetCollectionSets(collectionId bigint not null, setId bigint not null, createdTime datetime not null, modifiedTime datetime not null, primary key(collectionId, setId), constraint kf_collectionCardSetId foreign key (setId) references cardSet(id) on delete cascade, constraint kf_collectionId foreign key (collectionId) references cardSetCollection(id) on delete cascade);

create table playStats (id bigint not null auto_increment, userId bigint not null, setId bigint null, collectionId bigint null, passCount int not null, missCount int not null, playTime time(2) not null, createdTime datetime not null, primary key(id), constraint kf_playStatsUserId foreign key (userId) references users(id) on delete cascade, constraint kf_playStatCardSetId foreign key (setId) references cardSet(id) on delete cascade, constraint kf_playStatCollectionId foreign key (collectionId) references cardSetCollection(id) on delete cascade);
create table userSettings (userId bigint not null, colorCardPercent bool not null, colorCardThreshold int not null, showCardColorInGame bool not null, modifiedTime datetime not null, primary key(userId), constraint kf_settingsUserId foreign key (userId) references users(id) on delete cascade);
insert into userSettings(userId, colorCardPercent, colorCardThreshold, showCardColorInGame, modifiedTime)
select id, 1, 5, 0, UTC_TIMESTAMP() from users;
alter table card 
add column passCount int not null default 0 after backValue,
add column missCount int not null default 0 after passCount,
add column lastPass datetime after missCount,
add column lastMiss datetime after lastPass;
alter table cardSet
add column excludeFromStats bool not null default 0 after setName;