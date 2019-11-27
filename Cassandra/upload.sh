#!/bin/bash
password=`awk  '/default password/ {print $5}' bitnami_credentials | tr -d "'."`
cqlsh -u cassandra -p $password << eoc

DROP KEYSPACE IF EXISTS customerinfo;

CREATE KEYSPACE customerinfo
  WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

CREATE TABLE customerinfo.customerdetails (
  customerid int,
  firstname text,
  lastname text,
  email text,
  stateprovince text,
  PRIMARY KEY ((stateprovince), customerid)
);

DROP KEYSPACE IF EXISTS orderinfo;

CREATE KEYSPACE orderinfo
  WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

CREATE TABLE orderinfo.orderdetails (
  orderid int,
  customerid int,
  orderdate date,
  ordervalue decimal,
  PRIMARY KEY ((customerid), orderdate, orderid)
);

CREATE TABLE orderinfo.orderline (
  orderid int,
  orderline int,
  productname text,
  quantity smallint,
  orderlinecost decimal,
  PRIMARY KEY ((orderid), productname, orderline)
);

COPY customerinfo.customerdetails
  FROM 'customerdetails.dat';

COPY orderinfo.orderdetails
  FROM 'orderdetails.dat';

COPY orderinfo.orderline
  FROM 'orderline.dat';

eoc
