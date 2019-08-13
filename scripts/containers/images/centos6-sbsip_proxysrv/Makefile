include ../include/env.secrets.mk

VERSION ?= 7.0.0.0

IMAGE_NAME = centos6-sbsip_proxysrv
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
				-v $(SW):/opt/speechbridge/tmp \
				incendonet/azcopy \
				azcopy \
					--recursive --quiet \
					--source $(IMAGEFILESSOURCE)/proxysrv \
					--destination /opt/speechbridge/tmp \
					--source-sas $(AZURESASQUERYSTR_FORIMAGES)
	docker run \
				--rm \
				-v $(SW)/volumes:/opt/speechbridge/tmp \
				incendonet/azcopy \
				azcopy \
					--recursive --quiet \
					--source $(IMAGEFILESSOURCE)/volumes/opt-speechbridge-bin \
					--destination /opt/speechbridge/tmp \
					--source-sas $(AZURESASQUERYSTR_FORIMAGES)

	docker build -t quay.io/incendonet/centos6-sbsip_proxysrv -f Dockerfile .

clean:
	-rm -Rf $(SW)
	docker rmi $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):latest
	docker rmi $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):$(VERSION)
	docker rmi localhost:$(LOCAL_REPO_PORT)/$(IMAGE_NAME)
	docker rmi localhost:$(LOCAL_REPO_PORT)/$(IMAGE_NAME):$(VERSION)

include ../include/dockerrules.mk
