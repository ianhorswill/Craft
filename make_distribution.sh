rm -f /tmp/Craft_distribution /tmp/Craft_distribution.zip
mkdir /tmp/Craft_distribution
cp -r Documentation /tmp/Craft_distribution
cp Craft/bin/Release/Craft.dll /tmp/Craft_distribution
cp Unity/*.cs Unity/*.unity /tmp/Craft_distribution
cp *.md /tmp/Craft_distribution
zip -r /tmp/Craft_distribution.zip /tmp/Craft_distribution
