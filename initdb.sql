pragma foreign_keys=on;
begin transaction;
create table user (
  id integer primary key,
  username text not null unique,
  passwd text
);
create table session (
  id text primary key,
  userid integer not null,
  constraint fk_session_user
    foreign key (userid) references user(id) on delete cascade
);
create table category (
  id integer primary key,
  name text not null unique,
  color text not null
);
create table todo (
  id integer primary key,
  task text null,
  status integer default 0,
  due text default current_timestamp,
  userid integer not null,
  constraint fk_todo_user
    foreign key (userid) references user(id) on delete cascade
);
create table category_todo (
  categoryid integer not null,
  todoid integer not null,
  constraint fk_category_todo_category
    foreign key (categoryid) references category(id) on delete cascade,
  constraint fk_category_todo_todo
    foreign key (todoid) references todo(id) on delete cascade
);
commit;
