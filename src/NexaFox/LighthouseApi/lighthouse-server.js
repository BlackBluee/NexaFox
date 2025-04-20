const express = require('express');
const lighthouse = require('lighthouse');
const chromeLauncher = require('chrome-launcher');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

app.post('/audit', async (req, res) => {
    const url = req.body.url;

    try {
        const chrome = await chromeLauncher.launch({ chromeFlags: ['--headless'] });

        const options = {
            logLevel: 'info',
            output: 'html',
            onlyCategories: ['performance', 'accessibility', 'best-practices', 'seo'],
            port: chrome.port,
        };

        const runnerResult = await lighthouse(url, options);

        await chrome.kill();

        res.send(runnerResult.report);
    } catch (error) {
        console.error('Lighthouse error:', error);
        res.status(500).send(`Error running Lighthouse: ${error.message}`);
    }
});

app.listen(3000, () => console.log('Lighthouse API listening on port 3000'));
