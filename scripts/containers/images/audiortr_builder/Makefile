# Note:  We're assuming dependencies (SBLauncher and POCO-Foundation) are already built
# and the artifacts available.

include ../include/env.secrets.mk

VERSION ?= 7.0.0.0

IMAGE_NAME = audiortr_builder
REPO_HOST = quay.io
REPO_NAME = incendonet
LOCAL_REPO_PORT = 5000

BASE_DIR = /usr/local/src/incendonet/images
PROJ_DIR = $(BASE_DIR)/$(IMAGE_NAME)
ART_DIR = $(BASE_DIR)/artifacts
SW = $(PROJ_DIR)/software
GIT_NAME = AudioRtr2

GITPATH = "ssh://incendonet@vs-ssh.visualstudio.com:22/DefaultCollection/_git/$(GIT_NAME)/"

.PHONY: build clean

default: build

build: Dockerfile
	-rm -Rf $(PROJ_DIR)/.git
	-rm -Rf $(SW)
	-rm -Rf $(PROJ_DIR)/$(GIT_NAME)

	mkdir -p $(SW)
	git clone $(GITPATH)
	mv $(GIT_NAME)/libpjsipplusplus $(SW)
	mv $(GIT_NAME)/audiortr2 $(SW)

	docker build -t $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME) -f Dockerfile .
	docker create --name=$(IMAGE_NAME) $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME) /bin/true
	docker cp $(IMAGE_NAME):/usr/local/lib/ $(ART_DIR)/
	rm -f $(ART_DIR)/lib/*.a
	rm -Rf $(ART_DIR)/lib/pkgconfig
	docker cp $(IMAGE_NAME):/usr/local/src/audiortr2/audiortr2 $(ART_DIR)/
	docker rm $(IMAGE_NAME)

clean:
	-rm -Rf $(PROJ_DIR)/.git
	-rm -Rf $(SW)
	-docker rm $(IMAGE_NAME)
	-docker rmi $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME)
