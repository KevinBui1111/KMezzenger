-- USER SQL
CREATE USER KMEZZENGER IDENTIFIED BY "`12`12`12"  
DEFAULT TABLESPACE "USERS"
TEMPORARY TABLESPACE "TEMP";

-- QUOTAS
ALTER USER KMEZZENGER QUOTA UNLIMITED ON USERS;

-- ROLES

-- SYSTEM PRIVILEGES
GRANT CREATE VIEW TO KMEZZENGER ;
GRANT CREATE TABLE TO KMEZZENGER;
GRANT CREATE SESSION TO KMEZZENGER;
GRANT CREATE PROCEDURE TO KMEZZENGER;
GRANT CREATE SEQUENCE TO KMEZZENGER;


zaiIDM26RGgNklo4p1IlmzlGXM0=	fd0nD9fDtZ19BMiPaHJEhA==