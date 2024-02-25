sudo rm /etc/systemd/system/AssetValidationServiceV2.service;
sudo cp ./AssetValidationServiceV2.service /etc/systemd/system/;
sudo systemctl enable AssetValidationServiceV2.service;
sudo systemctl start AssetValidationServiceV2.service;
sudo systemctl status AssetValidationServiceV2.service;