include ../include/env.secrets.mk

VERSION ?= 7.0.0.0

IMAGE_NAME = sblauncher_builder
REPO_HOST = quay.io
REPO_NAME = incendonet
LOCAL_REPO_PORT = 5000

BASE_DIR = /usr/local/src/incendonet/images
PROJ_DIR = $(BASE_DIR)/$(IMAGE_NAME)
# Where the final artifacts go:
ART_DIR = $(BASE_DIR)/artifacts
# Where the dependencies needed to build the artifacts go:
SW = $(PROJ_DIR)/software

POCO_VER=1.7.8
POCO_PATCH=p3
POCO_SO_VER=48

PROPER_NAME=SBLauncher
GITPATH="ssh://incendonet@vs-ssh.visualstudio.com:22/DefaultCollection/_git/speechbridge/"
BRANCH=dev_sb7

.PHONY: build clean

default: build

build: Dockerfile
	rm -Rf ../.git
	mkdir -p $(SW)
	mkdir -p $(ART_DIR)
	git init
	-git remote add -f speechbridge ${GITPATH}
	git config core.sparsecheckout true
	echo $(PROPER_NAME) > .git/info/sparse-checkout
	git checkout $(BRANCH)
	rm -Rf .git
	cd $(PROJ_DIR)

	rm -Rf $(SW)/$(PROPER_NAME)
	mv $(PROPER_NAME) $(SW)/

	docker build \
		--build-arg POCO_VER=$(POCO_VER) \
		--build-arg POCO_PATCH=$(POCO_PATCH) \
		-t $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME) \
		-f Dockerfile \
		.
	docker create --name=$(IMAGE_NAME) $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME) /bin/true
	docker cp $(IMAGE_NAME):/usr/local/src/poco-$(POCO_VER)$(POCO_PATCH)/lib/Linux/x86_64/libPocoFoundation.so.$(POCO_SO_VER) $(ART_DIR)
	docker cp $(IMAGE_NAME):/usr/local/src/poco-$(POCO_VER)$(POCO_PATCH)/lib/Linux/x86_64/libPocoFoundation.so $(ART_DIR)
	docker cp $(IMAGE_NAME):/usr/local/src/poco-$(POCO_VER)$(POCO_PATCH)/Foundation/samples/$(PROPER_NAME)/bin/Linux/x86_64/$(PROPER_NAME) $(ART_DIR)
	docker rm $(IMAGE_NAME)

clean:
	cd
	-rm -Rf $(PROJ_DIR)/.git
	-rm -Rf $(SW)
	-docker rm $(IMAGE_NAME)
	-docker rmi $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME)
