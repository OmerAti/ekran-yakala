<?php
// Ömer ATABER - OmerAti omerati6363@gmail.com
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_FILES['file'])) {
    $uploadDir = './uploads/';
    $fileName = uniqid('screenshot_') . '.png';
    $uploadPath = $uploadDir . $fileName;
    if (move_uploaded_file($_FILES['file']['tmp_name'], $uploadPath)) {
        $imageUrl = 'https://' . $_SERVER['HTTP_HOST'] . dirname($_SERVER['REQUEST_URI']) . '/uploads/' . $fileName;
        echo json_encode(['success' => true, 'url' => $imageUrl]);
    } else {
        echo json_encode(['success' => false, 'error' => 'Dosya yüklenemedi']);
    }
} else {
    echo json_encode(['success' => false, 'error' => 'Geçersiz istek']);
}
?>
