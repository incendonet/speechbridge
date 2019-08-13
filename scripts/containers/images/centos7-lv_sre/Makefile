include ../include/env.secrets.mk

VERSION = 15.1.200-1

IMAGE_NAME = centos7-lv_sre
REPO_HOST = quay.io
REPO_NAME = incendonet
LOCAL_REPO_PORT = 5000

BASE_DIR = /usr/local/src/incendonet/images
PROJ_DIR = $(BASE_DIR)/$(IMAGE_NAME)
ART_DIR = $(BASE_DIR)/artifacts
THIRD_DIR = $(BASE_DIR)/thirdparty
SW = $(PROJ_DIR)/software

IMAGEFILESSOURCE = https://incendostor1.blob.core.windows.net/artifacts-forimages/SB_Zanzibar

.PHONY: build clean push-local push-remote tag

default: build tag

build:
	-mkdir -p $(SW)
	-cp -f $(THIRD_DIR)/LumenVox* $(SW)/

	docker run \
				--rm \
				-v $(SW):/opt/speechbridge/tmp \
				incendonet/azcopy \
				azcopy \
					--quiet \
					--source $(IMAGEFILESSOURCE)/volumes/opt-speechbridge-bin \
					--include envsub.sh \
					--destination /opt/speechbridge/tmp \
					--source-sas $(AZURESASQUERYSTR_FORIMAGES)
	docker run \
				--rm \
				-v $(SW):/opt/speechbridge/tmp \
				incendonet/azcopy \
				azcopy \
					--quiet \
					--source $(IMAGEFILESSOURCE)/volumes/opt-speechbridge-bin \
					--include entrypoint-lvsre.sh \
					--destination /opt/speechbridge/tmp \
					--source-sas $(AZURESASQUERYSTR_FORIMAGES)
	docker run \
				--rm \
				-v $(SW):/opt/speechbridge/tmp \
				incendonet/azcopy \
				azcopy \
					--quiet \
					--source $(IMAGEFILESSOURCE)/volumes/opt-speechbridge-templates \
					--include client_property.conf.TEMPLATE \
					--destination /opt/speechbridge/tmp \
					--source-sas $(AZURESASQUERYSTR_FORIMAGES)

	docker build \
		--build-arg VERSION=$(VERSION) \
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
