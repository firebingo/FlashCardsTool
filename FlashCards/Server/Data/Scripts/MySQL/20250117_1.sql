use {{dbname}};
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