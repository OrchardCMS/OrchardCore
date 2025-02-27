/*
 * Read https://cloud.google.com/translate/docs/quickstart-client-libraries
 * for Google APIs authentication
 */

const fs = require("fs");
const path = require("path");
var gettextParser = require("gettext-parser");
const args = require("yargs").argv;
var colors = require("colors");
const { Translate } = require("@google-cloud/translate").v2;

// Your Google Project Id
const projectId = args.project_id;

// PO / POT source folder
const po_source_path = args.po_source;

// PO destination folder
const po_path = args.po_dest;

// Target language
const target = args.lang;

if (!(projectId && po_source_path && po_path && target)) {
  showHelp();
  process.exit(-1);
}
if (!process.env.GOOGLE_APPLICATION_CREDENTIALS) {
  showGoogleHelp();
  process.exit(-1);
}

function showHelp() {
  let usage = "Usage: node " + args.$0 + " --project_id={your google project id} ";
  usage += "--po_source={folder path of empty PO file} ";
  usage += "--po_dest={folder path of translated PO file} ";
  usage += "--lang={language code} ";
  console.log(usage.red);

  console.log("");
  let example = "Example: node " + args.$0 + " --project_id=example ";
  example += "--po_source=/Users/diego/";
  example += "--po_dest=/Users/diego/output ";
  example += "--lang=it";

  console.log(example);
}

function showGoogleHelp() {
  console.log("Environment variable GOOGLE_APPLICATION_CREDENTIALS is empty".red);
  console.log("Help: https://cloud.google.com/translate/docs/quickstart-client-libraries");
}

const translate = new Translate({ projectId });

async function tr(text) {
  const [translation] = await translate.translate(text, target);
  return translation;
}

async function start() {
  // create output folder if it does not exist
  if (!fs.existsSync(po_path)) {
    fs.mkdirSync(po_path);
  }

  fs.readdirSync(po_source_path).forEach(async (file) => {
    var filePath = path.parse(file);
    if (!(filePath.ext == ".po" || filePath.ext == ".pot")) {
      return;
    }

    console.log("Loading file: ", file);
    var input = fs.readFileSync(path.join(po_source_path, file));

    var po = gettextParser.po.parse(input, { defaultCharset: "utf8" });

    var outputPath = path.join(po_path, filePath.name + ".po");
    console.log("Output:", outputPath);

    if (fs.existsSync(outputPath)) {
      var output = fs.readFileSync(outputPath);
      var tpo = gettextParser.po.parse(output, { defaultCharset: "utf8" });
    }

    for (let k in po.translations) {
      for (let l in po.translations[k]) {
        if (l) {
          if (tpo && tpo.translations[k] && tpo.translations[k][l] && tpo.translations[k][l].msgstr[0] != "") {
            //we have a translation already for this output file
            console.log(`Already have a translation for: ${l} `);
            po.translations[k][l].msgstr[0] = tpo.translations[k][l].msgstr[0];
          } else {
            console.log(`Text: ${l}`);
            let translation = await tr(l);
            console.log(`Translation: ${translation}`);
            po.translations[k][l].msgstr[0] = translation;
          }
        }
      }
    }

    var output_po = gettextParser.po.compile(po, { foldLength: false });

    fs.writeFileSync(outputPath, output_po, { defaultCharset: "utf8" });
  });
}

start();
