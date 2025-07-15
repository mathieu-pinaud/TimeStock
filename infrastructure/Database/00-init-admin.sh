#!/bin/bash
set -e

# Ce script est exécuté par /usr/local/bin/docker-entrypoint.sh (root)
# Les variables MYSQL_ROOT_PASSWORD et ADMIN_PASSWORD sont injectées par compose

mysql -u root -p"$MYSQL_ROOT_PASSWORD" <<-EOSQL
  CREATE USER IF NOT EXISTS 'timestock_admin'@'%' IDENTIFIED BY '${ADMIN_PASSWORD}';
  GRANT ALL PRIVILEGES ON *.* TO 'timestock_admin'@'%' WITH GRANT OPTION;
  FLUSH PRIVILEGES;
EOSQL
