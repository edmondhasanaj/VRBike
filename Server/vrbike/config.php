<?php
    $server = "localhost";
    $db = "vrbike";
    $username = "root";
    $password = "";  
    
    function createDBConnection(){
        global $server,$db,$username,$password;
        $db = new PDO("mysql:host=".$server.";dbname=".$db.";charset=utf8mb4", $username, $password);      
        $db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        return $db;
    }
?>