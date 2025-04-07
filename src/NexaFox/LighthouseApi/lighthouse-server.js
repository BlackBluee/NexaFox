const express = require('express');
const lighthouse = require('lighthouse');
const chromeLauncher = require('chrome-launcher');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

app.post('/audit', async (req, res) => {
    const url = req.body.url;

    const chrome = await chromeLauncher.launch({ chromeFlags: ['--headless'] });
    const result = await lighthouse(url, {
        port: chrome.port,
        output: 'html',
    });

    await chrome.kill();

    res.send(result.report);
});

app.listen(3000, () => console.log('Lighthouse API listening on port 3000'));
