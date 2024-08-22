pragma foreign_keys=on;

/*
  scratch file i use to quickly test sql queries, 
  run ./x.sh -w (runs watch), check ./x.sh to see how this works.
*/


/*
.mode
ascii box column csv html insert json line list markdown qbox
quote table tabs tcl

.mode json


.tables


update or ignore category
  set name = 'duplicate', color = 'cfd3d7'
where id = 2
update or ignore category set name = 'duplicatez' where id = 2
delete from category where id = 77
delete from category
*/

select * from category 


/*

┌────┬─────────────┬────────┐
│ id │    name     │ color  │
├────┼─────────────┼────────┤
│ 1  │ bug         │ d73a4a │
│ 2  │ duplicate   │ cfd3d7 │
│ 3  │ enhancement │ a2eeef │
└────┴─────────────┴────────┘
*/
