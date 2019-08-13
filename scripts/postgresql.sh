rpm -iv /home/speechbridge/software/postgresql/postgresql-libs-8.2.3-1PGDG.i686.rpm
rpm -iv /home/speechbridge/software/postgresql/postgresql-8.2.3-1PGDG.i686.rpm
rpm -iv /home/speechbridge/software/postgresql/postgresql-server-8.2.3-1PGDG.i686.rpm
rpm -iv /home/speechbridge/software/postgresql/postgresql-devel-8.2.3-1PGDG.i686.rpm
rpm -iv /home/speechbridge/software/postgresql/postgresql-docs-8.2.3-1PGDG.i686.rpm
service postgresql initdb
service postgresql start
