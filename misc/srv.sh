#!/usr/bin/zsh
set -euo pipefail

#usage ./x.sh -pbx
#./x.sh -p will run purge, -b runs build and so on
#./x.sh -pbx will run purge bld and xxx in succession


purge() {
  #rm -rf bin obj
  #dotnet clean -c Release api.csproj
}

bld() {
  #dotnet build  \
  dotnet build --source ${HOME}/.nuget/packages \
    -r linux-x64 -c Release api.csproj
}

xxx() {
  ./bin/Release/net9.0/linux-x64/api
}

addpkg() {
  exit
  dotnet add package Microsoft.Data.Sqlite
}

_publish() {
  rm -rfv out || :
  dotnet publish -r linux-x64 -c Release -o out api.csproj
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
