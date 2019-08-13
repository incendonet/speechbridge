VERSION = 6.0.1

IMAGE_NAME = centos7-mono-lv_client-cep
REPO_HOST = quay.io
REPO_NAME = incendonet
LOCAL_REPO_PORT = 5000

BASE_DIR = /usr/local/src/incendonet/images
PROJ_DIR = $(BASE_DIR)/$(IMAGE_NAME)
ART_DIR = $(BASE_DIR)/artifacts
THIRD_DIR = $(BASE_DIR)/thirdparty
SW = $(PROJ_DIR)/software

.PHONY: build clean push-local push-remote tag

default: build tag

build: Dockerfile  $(ART_DIR)/CepstralCmd
	-mkdir -p $(SW)
	-cp -f $(THIRD_DIR)/Cepstral_Callie-8kHz* $(SW)/
	-cp -f $(ART_DIR)/CepstralCmd $(SW)/

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
