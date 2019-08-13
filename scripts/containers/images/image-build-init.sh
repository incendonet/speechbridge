#!/bin/bash

set +e

SUBPATH="scripts/containers/images"
BASEPATH="/usr/local/src/incendonet/images"
GITPATH="ssh://incendonet@vs-ssh.visualstudio.com:22/DefaultCollection/_git/speechbridge/"
BRANCH="dev_sb7"

mkdir -p ${BASEPATH}
cd ${BASEPATH}

rm -Rf .git
rm -Rf include
rm -Rf thirdparty
rm -Rf centos*
rm -Rf sb*
rm -Rf audiortr_builder
rm -Rf azcopy
rm -Rf cepstralcmd_builder

git init
git remote add -f speechbridge ${GITPATH}
git config core.sparsecheckout true
echo ${SUBPATH} > .git/info/sparse-checkout
git checkout ${BRANCH}
mv -f ${SUBPATH}/* .
rm -Rf scripts

cd thirdparty
chmod +x download-thirdparty.sh
./download-thirdparty.sh
