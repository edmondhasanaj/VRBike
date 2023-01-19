create database vrbike;
use vrbike;
drop table users;
create table users (
username varchar(255), 
password varchar(255),

primary key(username)
);

create table login_attempts(
id int primary key auto_increment,
username varchar(255),
password varchar(255)
);

delimiter $$
drop function if exists username_check_format$$
create function username_check_format(pUsername varchar(255))
returns bool
begin
	declare valid int;
	select pUsername regexp "^[a-zA-Z0-9]{12,24}$" into valid;
    return valid;
end$$

drop function if exists password_check_format$$
create function password_check_format(pPassword varchar(255))
returns bool
begin
	declare valid int;
	select pPassword regexp "^[a-zA-Z0-9!#$%^&*]{12,24}$" into valid;
    return valid;
end$$

-- ECode = 0 -> Successful
-- ECode = 1 -> Input not in required format
-- ECode = 2 -> User exists already
drop procedure if exists create_user$$
create procedure create_user(in pUsername varchar(255), in pPassword varchar(255), out eCode int)
create_user:begin    
	declare sameUsername int;
	
    -- Check if username and password are in the required format
    if username_check_format(pUsername) = 0 or password_check_format(pPassword) = 0
    then 
		set eCode = 1;
        leave create_user;
	end if;
    
    -- Check if user exists
	select count(*) into sameUsername from users
    where username = md5(pUsername);
    if sameUsername != 0
    then 
		set eCode = 2;
        leave create_user;
	end if;
    
    -- Create user
    insert into users values (md5(pUsername), md5(pPassword));
    set eCode = 0;
end$$

-- ECode = 0 -> Successful
-- ECode = 1 -> Input not in the required format
-- ECode = 2 -> Credentials not right 
drop procedure if exists login_user$$
create procedure login_user(in pUsername varchar(255), in pPassword varchar(255), out eCode int)
login_user:begin
	declare sameUsername int;

    insert into login_attempts (username, password) values (pUsername, pPassword);

    -- Check if username and password are in the required format
    if username_check_format(pUsername) = 0 or password_check_format(pPassword) = 0
    then 
		set eCode = 1;
        leave login_user;
	end if;
    
    -- Check if user wasn't found
	select count(*) into sameUsername from users
    where username = md5(pUsername) and password = md5(pPassword);
    if sameUsername != 1
    then 
		set eCode = 2;
        leave login_user;
	end if;
    
    -- Successful
    set eCode = 0;
end$$

-- ECode = 0 -> Successful
-- ECode = 1 -> Input not in the required format
-- ECode = 2 -> User doesn't exist
drop procedure if exists delete_user$$
create procedure delete_user(in pUsername varchar(255), out eCode int)
delete_user:begin
	declare sameUsername int;

    -- Check if username is in the required format
    if username_check_format(pUsername) = 0
    then 
		set eCode = 1;
        leave delete_user;
	end if;
    
    -- Check if user wasn't found
	select count(*) into sameUsername from users
    where username = md5(pUsername);
    if sameUsername != 1
    then 
		set eCode = 2;
        leave delete_user;
	end if;
    
    -- Successful. Delete user
    delete from users where username = pUsername;
    set eCode = 0;
end$$
delimiter ;