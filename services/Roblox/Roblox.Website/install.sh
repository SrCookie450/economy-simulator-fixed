sudo rm /etc/systemd/system/RobloxWeb.service;
sudo cp ./RobloxWeb.service /etc/systemd/system/;
sudo systemctl enable RobloxWeb.service;
sudo systemctl start RobloxWeb.service;
sudo systemctl status RobloxWeb.service;