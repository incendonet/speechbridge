include ../include/env.secrets.mk

VERSION ?= 7.0.0.0

IMAGE_NAME = centos7-sbbin_lv_cep
REPO_HOST = quay.io
REPO_NAME = incendonet
LOCAL_REPO_PORT = 5000

BASE_DIR = /usr/local/src/incendonet/images
PROJ_DIR = $(BASE_DIR)/$(IMAGE_NAME)
ART_DIR = $(BASE_DIR)/artifacts
SW = $(PROJ_DIR)/software

IMAGEFILESSOURCE = https://incendostor1.blob.core.windows.net/artifacts-forimages/SB_Zanzibar

.PHONY: build clean push-local push-remote tag

default: build tag

build: Dockerfile
	mkdir -p $(SW)

	docker run \
				--rm \
				-v $(SW)/dotnet:/opt/speechbridge/tmp \
				incendonet/azcopy \
				azcopy \
					--recursive --quiet \
					--source $(IMAGEFILESSOURCE)/dotnet \
					--destination /opt/speechbridge/tmp \
					--source-sas $(AZURESASQUERYSTR_FORIMAGES)
	docker run \
				--rm \
				-v $(SW)/volumes:/opt/speechbridge/tmp \
				incendonet/azcopy \
				azcopy \
					--recursive --quiet \
					--source $(IMAGEFILESSOURCE)/volumes \
					--destination /opt/speechbridge/tmp \
					--source-sas $(AZURESASQUERYSTR_FORIMAGES)

	docker build \
		-t $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME) \
		-f Dockerfile \
		.

clean:
	-rm -Rf $(SW)
	docker rmi $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):latest
	docker rmi $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):$(VERSION)
	docker rmi localhost:$(LOCAL_REPO_PORT)/$(IMAGE_NAME)
	docker rmi localhost:$(LOCAL_REPO_PORT)/$(IMAGE_NAME):$(VERSION)


include ../include/dockerrules.mk
