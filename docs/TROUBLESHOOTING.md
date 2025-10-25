

# Install dotnet runtime on Raspberry Pi Zero 2 W
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --verbose --install-dir "/home/admin/.dotnet/" --zip-path "/home/admin/tmp/dotnet.tar.gz" --no-path --runtime dotnet

echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Install WiringPi
sudo apt-get update
git clone https://github.com/WiringPi/WiringPi
cd WiringPi
./build

gpio -v
gpio readall

# Enable i2c

sudo-raspi-config
3 Interface Options
I5 I2C
<Yes>
<Ok>
Esc

i2cdetect -y 1