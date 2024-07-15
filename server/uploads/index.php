<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Anlık Ekran Görüntüsü</title>
    <meta name="robots" content="noindex, nofollow" />
    <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;700&display=swap" rel="stylesheet">
    <style>
        body {
            font-family: 'Roboto', sans-serif;
            background-color: #eef2f7;
            color: #333;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            height: 100vh;
            margin: 0;
            padding: 0;
        }
        h1 {
            margin: 20px 0;
            color: #444;
        }
        .container {
            text-align: center;
            padding: 30px;
            background: #fff;
            border-radius: 15px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
            transition: all 0.3s ease;
        }
        .container:hover {
            box-shadow: 0 6px 25px rgba(0, 0, 0, 0.15);
        }
        img {
            max-width: 100%;
            height: auto;
            border-radius: 15px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
        }
        .error {
            color: #e74c3c;
        }
        .logo {
            width: 150px;
            height: auto;
            margin-bottom: 20px;
        }
        footer {
            margin-top: 20px;
            font-size: 0.9em;
            color: #888;
        }
        footer a {
            color: #3498db;
            text-decoration: none;
        }
        footer a:hover {
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <div class="container">
        <img class="logo" src="https://screenshot.domainadi.com/logo-s.png" alt="Logo">
        <h1>Anlık Ekran Görüntüsü</h1>
        <?php
        $encid = isset($_GET['encid']) ? $_GET['encid'] : '';
        if (!empty($encid)) {
            echo '<img src="https://screenshot.domainadi.com/uploads/' . htmlspecialchars($encid, ENT_QUOTES, 'UTF-8') . '" alt="Resim">';
        } else {
            echo '<p class="error">Resim bulunamadı.</p>';
        }
        ?>
        <footer>
            Domainadi.Com Internet Hizmetleri
            <br>
            <a href="mailto:info@domainadi.com">info@domainadi.com</a>
        </footer>
    </div>
</body>
</html>
