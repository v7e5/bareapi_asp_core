#!/usr/bin/zsh
set -euo pipefail

cke='./cookie'

ccc() {
  local _i=0
  [[ ! -f ${cke} ]] && touch ${cke}

  local a=(
    category/list
    user/list
    user/create
    user/delete
    category/delete
    category/create
    category/update
    'echo'
    env
    todo/list
    dummy
    category/list
    login
    'logout'
    dash
    sess
    user/list
    todo/create
    user/create
  )

  #-o /dev/null \
  #--write-out "@./write_out_fmt.yml" \
    #--data-binary "$(./q.sh -x)" \
  curl -vs -X POST \
    --cookie ${cke} \
    --cookie-jar ${cke} \
    -H 'content-type: application/json' \
    -H 'accept: application/json' \
    --data-binary "$(./q.sh -x)" \
    'http://0.0.0.0:8000/'${a[1]} | jq
}

w() {
  local a=(
    x.sh
    x.sql
  )

  inotifywait -mr -e close_write -e delete -e moved_to ./ \
    | while read d e f; do
        if [[ $a[(Ie)${f}] -ne 0 ]]; then
          clear -x
          cl -b 27  -f 51 -o '------------------------------------------------'
          echo
          case ${f} in
            x.sh) ./x.sh -c || :;;
            x.sql) cat ${f} | sqlite3 api.db || :;;
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
