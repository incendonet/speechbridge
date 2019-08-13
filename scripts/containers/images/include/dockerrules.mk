tag:
	docker tag $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):latest $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):$(VERSION)
	docker tag $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):latest localhost:$(LOCAL_REPO_PORT)/$(IMAGE_NAME)
	docker tag $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):latest localhost:$(LOCAL_REPO_PORT)/$(IMAGE_NAME):$(VERSION)

push-local: tag
	docker push localhost:$(LOCAL_REPO_PORT)/$(IMAGE_NAME):latest
	docker push localhost:$(LOCAL_REPO_PORT)/$(IMAGE_NAME):$(VERSION)

push-remote: tag
	docker push $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):latest
	docker push $(REPO_HOST)/$(REPO_NAME)/$(IMAGE_NAME):$(VERSION)
