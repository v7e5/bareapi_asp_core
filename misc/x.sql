pragma foreign_keys=on;

/*
  scratch file i use to quickly test sql queries, 
  run ./x.sh -w (runs watch), check ./x.sh to see how this works.

.mode
ascii box column csv html insert json line list markdown qbox
quote table tabs tcl

.tables

*/
.mode table



select * from dummy limit 1;

/*

create table dummy (
  id integer primary key,
  word text not null
);
insert into dummy(word) values
('abdomen'),
('baching'),
('cabezon'),
('dabster'),
('earbuds'),
('fablers'),
('gabfest'),
('habited'),
('iceboat'),
('jabirus'),
('kacheri'),
('labored'),
('machers'),
('nackets'),
('oakiest'),
('pablums'),
('qindars'),
('rabidly'),
('sabeing'),
('taberds'),
('ugliest'),
('vacking'),
('wabster'),
('xanthic'),
('yachted'),
('zabtieh');

select
  c.id, c.grupo, c.hex, c.grade, c.vivid
from color c where 1 
  and (
      c.id like %10%
      or c.grupo like '%ff%'
      or c.hex like '%ass%'
      or c.grade like '%ass%'
    )
group by c.id order by c.id asc


  and c.id > 105
alter table user rename column passwd to password;

select * from user;

select
  t.id, t.task, t.done, t.due_unix_timestamp,
  json_group_array(
    json_object('id', c.id, 'name', c.name, 'color', c.color))
    as categories
from todo t
  left join category_todo ct on t.id = ct.todoid
  left join category c on c.id = ct.categoryid
where t.userid = 3
group by t.id order by t.due_unix_timestamp desc limit 10
.mode json

select id, task, done, datetime(due_unix_timestamp, 'unixepoch')
  from todo where userid = 3 and id=100;


update todo set task = 'centum' where id = 100;

*/


/*
select * from category_todo where categoryid=8;
select
  t.id,
  t.task,
  t.done,
  t.due_unix_timestamp,
  json_group_array(
    json_object('id', c.id, 'name', c.name, 'color', c.color)
  ) as categories
from todo t
  left join category_todo ct on t.id = ct.todoid
  left join category c on c.id = ct.categoryid
where 1 
  and t.task like '%hour%'
  and ct.categoryid in (3)
  and t.userid = 3
group by t.id order by t.id asc 
*/


/*

select * from category_todo where todoid = 100;

update or ignore todo set 
  task = '[Feature request] better playlist RSS feeds',
  done = 0
  where id = 100 and userid = 3

[{"id":100,"task":"[Feature request] better playlist RSS feeds",
"done":0,"datetime(due_unix_timestamp, 'unixepoch')":"2026-10-20 20:28:52"}]


insert into az(letter, due) values
('a', 111),
('b', 112),
('c', 113),
('d', 114),
('e', 115),
('f', 116),
('g', 117),
('h', 118),
('i', 119),
('j', 120),
('k', 121),
('l', 122),
('m', 123),
('n', 124),
('o', 125),
('p', 126),
('q', 127),
('r', 128),
('s', 129),
('t', 130),
('u', 130),
('v', 132),
('w', 133),
('x', 134),
('y', 135),
('z', 136)
create table az (
  id integer primary key,
  letter text not null,
  due integer not null
);

insert into user(username, passwd) values
('admin', 'forget'),
('kjv', 'forget'),
('xxx', 'forget')

*/
