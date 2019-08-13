include ../include/env.secrets.mk

VERSION = 6.0.1

IMAGE_NAME = cepstralcmd_builder
REPO_HOST = quay.io
REPO_NAME = incendonet
LOCAL_REPO_PORT = 5000

BASE_DIR = /usr/local/src/incendonet/images
PROJ_DIR = $(BASE_DIR)/$(IMAGE_NAME)
THIRD_DIR = $(BASE_DIR)/thirdparty
# Where the final artifacts go:
ART_DIR = $(BASE_DIR)/artifacts
# Where the dependencies needed to build the artifacts go:
SW = $(PROJ_DIR)/software

GITPATH = "ssh://incendonet@vs-ssh.visualstudio.com:22/DefaultCollection/_git/speechbridge/"

.PHONY: build clean

build: Dockerfile
	-mkdir -p $(SW)
	-cp -f $(THIRD_DIR)/Cepstral_Callie-8kHz* $(SW)/
	-cp -f $(ART_DIR)/CepstralCmd $(SW)/

	mkdir -p $(ART_DIR)
	cd $(SW)
	git init
	-git remote add -f speechbridge $(GITPATH)
	git fetch $(GITPATH)
	git checkout FETCH_HEAD -- CepstralCmd/Makefile
	git checkout FETCH_HEAD -- CepstralCmd/CepstralCmd.c
	mv -f CepstralCmd/* $(SW)
	rm -Rf CepstralCmd
	docker build \
		--build-arg VERSION=$(VERSION) \
		-t $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME) \
		-f Dockerfile .
	docker create --name=$(IMAGE_NAME) $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME) /bin/true
	docker cp $(IMAGE_NAME):/usr/local/src/cepstralcmd/CepstralCmd $(ART_DIR)
	docker rm $(IMAGE_NAME)

clean:
	cd
	-rm -Rf $(PROJ_DIR)/.git
	-rm -Rf $(SW)
	-docker rm $(IMAGE_NAME)
	-docker rmi $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME)

default: build
