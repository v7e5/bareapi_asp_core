#!/usr/bin/zsh
set -euo pipefail

cke='./misc/cookie'

bulk() {
  while IFS= read -r l; do
    curl -vs -X POST \
      --cookie ${cke} \
      --cookie-jar ${cke} \
      -H 'content-type: application/json' \
      -H 'accept: application/json' \
      --data-binary "${l}" \
      'http://0.0.0.0:8000/todo/create' || :
  done <./misc/todo_2.txt
}

ccc() {
  local _i=0
  [[ ! -f ${cke} ]] && touch ${cke}

  local a=(
    hailstone
    env
    user/profile
    now
    login
    'logout'
    category/list
    user/resetpass
    user/create
    todo/list
    todo/delete
    user/delete
    user/list
    todo/create
    az
    todo/update
    category/create
    category/delete
    category/update
    'echo'
    env
  )

  local u='http://0.0.0.0:8000/'${a[1]}

  local m=l

  if [[ ${m} == p ]]; then

    cl -o -f 4 ${u}'\n'

    curl -vs -X POST \
      --cookie ${cke} \
      --cookie-jar ${cke} \
      -H 'content-type: application/json' \
      -H 'accept: application/json' \
      --data-binary "$(./misc/q.sh -x)" ${u} | jq
  else
    u=${u}'?'$(./misc/q.sh -x \
        | node -r fs -e 'process.stdout.write(('\
'new URLSearchParams(JSON.parse(fs.readFileSync(0, "utf-8")))).toString())')

    cl -o -f 3 ${u}'\n'

    curl -vs -X GET \
      --cookie ${cke} \
      --cookie-jar ${cke} \
      -H 'accept: application/json' ${u} | jq
  fi
}

w() {
  local a=(
    x.sh
    q.sh
    x.sql
  )

  inotifywait -mr -e close_write -e delete -e moved_to ./ \
    | while read d e f; do
        if [[ $a[(Ie)${f}] -ne 0 ]]; then
          clear -x
          cl -b 27  -f 51 -o '------------------------------------------------'
          echo
          case ${f} in
            x.sh) ./misc/x.sh -c || :;;
            q.sh) ./misc/x.sh -c || :;;
            x.sql) cat ./misc/${f} | sqlite3 api.db || :;;
          esac
        fi
      done
}

_k=(${(ok)functions:#_*})
_v=(${(oM)_k#[a-z]*})
typeset -A _o
_o=(${_v:^_k})

eval 'zparseopts -D -E -F -a _a '${_v}

[[ ${#_a} -eq 0  ]] && \
  paste -d ' ' <(print -l '\-'${(j:\n-:)_v}) <(print -l ${_k}) && exit

_a=('$_o['${^_a#-}']')
eval ${(F)_a}
exit
