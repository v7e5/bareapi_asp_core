pragma foreign_keys=on;

/*
  scratch file i use to quickly test sql queries, 
  run ./x.sh -w (runs watch), check ./x.sh to see how this works.

.mode
ascii box column csv html insert json line list markdown qbox
quote table tabs tcl

*/
.mode table

insert into az(word) values 
  ('filler_a'),
  ('filler_b'),
  ('filler_c'),
  ('filler_d');

select * from az;

/*
update az set word = 'abdomen_04' where id = 1;
update az set word = 'baching_10' where id = 2; 
update az set word = 'cabezon_04' where id = 3; 
update az set word = 'dabster_10' where id = 4; 
update az set word = 'earbuds_11' where id = 5; 
update az set word = 'fablers_10' where id = 6; 
update az set word = 'gabfest_11' where id = 7; 
update az set word = 'habited_10' where id = 8; 
update az set word = 'iceboat_11' where id = 9; 
update az set word = 'jabirus_10' where id = 10;
update az set word = 'kacheri_11' where id = 11;
update az set word = 'labored_10' where id = 12;
update az set word = 'machers_11' where id = 13;
update az set word = 'nackets_10' where id = 14;
update az set word = 'oakiest_11' where id = 15;
update az set word = 'pablums_10' where id = 16;
update az set word = 'qindars_11' where id = 17;
update az set word = 'rabidly_10' where id = 18;
update az set word = 'sabeing_11' where id = 19;
update az set word = 'taberds_10' where id = 20;
update az set word = 'ugliest_11' where id = 21;
update az set word = 'vacking_04' where id = 22;
update az set word = 'wabster_11' where id = 23;
update az set word = 'xanthic_04' where id = 24;
update az set word = 'yachted_11' where id = 25;
*/

/*
.tables
*/
