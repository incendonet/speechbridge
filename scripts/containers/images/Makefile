# Build all speechbridge docker images

VERSION ?= 7.0.0.0

.PHONY: build clean push-local push-remote tag

build:
	docker pull centos:6
	docker pull centos:7
	docker pull microsoft/aspnetcore:1.1.2
	docker pull postgres:9.2
	(cd centos6-sbsip_proxysrv && make build)
	(cd sblauncher_builder && make build)
	(cd audiortr_builder && make build)
	(cd centos7-sbsip_audiortr && make build)
	(cd centos7-lv_license && make build)
	(cd centos7-lv_sre && make build)
	(cd centos7-mono-lv_client && make build)
	(cd cepstralcmd_builder && make build)
	(cd centos7-mono-lv_client-cep && make build)
	(cd centos7-sbbin_lv_cep && make build)
	(cd centos7-sbweb && make build)
	(cd sblicenseserver && make build)

clean:
	-(cd centos6-sbsip_proxysrv && make clean)
	-(cd centos7-sbsip_audiortr && make clean)
	-(cd centos7-lv_license && make clean)
	-(cd centos7-lv_sre && make clean)
	-(cd centos7-mono-lv_client && make clean)
	-(cd centos7-sbbin_lv && make clean)
	-(cd centos7-sbbin_lv_cep && make clean)
	-(cd centos7-sbweb && make clean)
	-(cd sblicenseserver && make clean)
	-(cd audiortr_builder && make build)
	-(cd sblauncher_builder && make build)
	-(cd cepstralcmd_builder && make build)

push-local:
	(cd centos6-sbsip_proxysrv && make push-local)
	(cd centos7-sbsip_audiortr && make push-local)
	(cd centos7-lv_license && make push-local)
	(cd centos7-lv_sre && make push-local)
	(cd centos7-mono-lv_client && make push-local)
	(cd centos7-sbbin_lv_cep && make push-local)
	(cd centos7-sbbin_lv_cep && make push-local)
	(cd centos7-sbweb && make push-local)
	(cd sblicenseserver && make push-local)

push-remote:
	(cd centos6-sbsip_proxysrv && make push-remote)
	(cd centos7-sbsip_audiortr && make push-remote)
	(cd centos7-lv_license && make push-remote)
	(cd centos7-lv_sre && make push-remote)
	(cd centos7-mono-lv_client && make push-remote)
	(cd centos7-mono-lv_client-cep && make push-remote)
	(cd centos7-sbbin_lv_cep && make push-remote)
	(cd centos7-sbweb && make push-remote)
	(cd sblicenseserver && make push-remote)

tag:
	(cd centos6-sbsip_proxysrv && make tag)
	(cd centos7-sbsip_audiortr && make tag)
	(cd centos7-lv_license && make tag)
	(cd centos7-lv_sre && make tag)
	(cd centos7-mono-lv_client && make tag)
	(cd centos7-mono-lv_client-cep && make tag)
	(cd centos7-sbbin_lv_cep && make tag)
	(cd centos7-sbweb && make tag)
	(cd sblicenseserver && make tag)

default: build
