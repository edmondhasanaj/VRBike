<?php
    //Output List:
    //0 - User logged in successfully
    //1 - Username or password were not in the specified format
    //2 - Failed to log in
    //3 - Connection Error
    require_once("config.php");
    $db = createDBConnection();
    
    $username = isset($_POST["username"]) && !empty($_POST["username"]) && preg_match("/^[a-zA-Z0-9]{12,24}$/", $_POST["username"]) ? $_POST["username"] : null;
    $password = isset($_POST["password"]) && !empty($_POST["password"]) && preg_match("/^[a-zA-Z0-9!#$%^&*]{12,24}$/", $_POST["password"]) ? $_POST["password"] : null;

    if($username == null || $password == null)
    {
        http_response_code(400);
        exit();
    }

    $stmt = $db->prepare("call login_user(:username, :password, @output);");
    $stmt->bindParam(":username", $username);
    $stmt->bindParam(":password", $password);
    $stmt->execute();

    $stmt = $db->prepare("select @output as output;");
    $stmt->execute();
    $row = $stmt->fetch(PDO::FETCH_ASSOC);

    if($row)
    {
        if($row["output"] == "0")  
            http_response_code(200);
        else if($row["output"] == "1")
            http_response_code(400);
        else if($row["output"] == "2")
            http_response_code(409);
        else 
            http_response_code(500);
    }
    else
    {
        http_response_code(500);
    }
?>