import * as React from 'react';
import { hot } from 'react-hot-loader';

const App: React.SFC = () =>
    <div>
        Hello, hot reloading 5
    </div>;

export default hot(module)(App);
