
# Translate.ps1

Automated powershell script to translate Orchard Core.

## Usage:

```cmd
./translate.ps1
```

## Requirements

Google Translate API: see below.

# po-gtranslator

A node.js application for translating PO files using Google Translate API.

## Usage:

```cmd
$ node po-gtranslator.js --project_id={your google project id} --po_source={path of empty PO file} --po_dest={path of translated PO file} --mo={path of translated MO file} --lang={language code}
```

Example: 

```cmd
$ node po-gtranslator.js --project_id=example --po_source=/Users/diego/it-source.po --po_dest=/Users/diego/it.po --mo=/Users/diego/it.mo --lang=it
```

## Requirements

In order to use Google Cloud Translation API you need to setup a project in your Google Cloud Console, please check a simple guide here: https://cloud.google.com/translate/docs/quickstart-client-libraries

You will need to create a Service Account and Private Key that you will be able to download as a JSON file. The content of this file needs to be pasted inside the google-credentials.json file standing in this folder.
