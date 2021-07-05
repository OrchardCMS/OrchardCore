require('../scss/styles.scss');
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import ReactDiff, { DiffMethod } from 'react-diff-viewer';

interface DiffViewerState {
    splitView?: boolean;
    highlightLine?: string[];
    language?: string;
    enableSyntaxHighlighting?: boolean;
    compareMethod?: DiffMethod;
    old: string;
    new: string;
    useDarkMode: boolean;
    leftText: string;
    oldText: string;
    newText: string;
    split: boolean;
    splitText: string;
    unifiedText: string;
}

const P = (window as any).Prism;

class DiffViewer extends React.Component<{}, DiffViewerState> {
    public constructor(props: any) {
        super(props);
        this.state = {
            highlightLine: [],
            enableSyntaxHighlighting: true,
            old: '',
            new: '',
            leftText: '',
            oldText: '',
            newText: '',
            useDarkMode: false,
            split: true,
            splitText: '',
            unifiedText: ''
        };
        this.onSplitChange = this.onSplitChange.bind(this);
    }
  
    onSplitChange(event) {
        const split = event.target.value === "split" ? true : false
        this.setState({
            split: split,
            leftText: split ? this.state.oldText : this.state.oldText + '\n' + this.state.newText
        })
    }

    componentDidMount() {
        const darkMode = document.documentElement.getAttribute('data-theme');
        const mountId = document.getElementById('diffapp');
        const oldText = mountId.getAttribute('data-old-text') + ' - ' + mountId.getAttribute('data-old-time');
        
        this.setState({
            old: mountId.getAttribute('data-old'),
            new: mountId.getAttribute('data-new'),
            oldText: oldText,
            leftText: oldText,
            newText: mountId.getAttribute('data-new-text') + ' - ' + mountId.getAttribute('data-new-time'),
            useDarkMode: document.documentElement.getAttribute('data-theme').toLowerCase() === 'darkmode' ? true : false,
            splitText: mountId.getAttribute('data-split'),
            unifiedText: mountId.getAttribute('data-unified'),
        });
    }

    private onLineNumberClick = (
        id: string,
        e: React.MouseEvent<HTMLTableCellElement>,
    ): void => {
        let highlightLine = [id];
        if (e.shiftKey && this.state.highlightLine.length === 1) {
            const [dir, oldId] = this.state.highlightLine[0].split('-');
            const [newDir, newId] = id.split('-');
            if (dir === newDir) {
                highlightLine = [];
                const lowEnd = Math.min(Number(oldId), Number(newId));
                const highEnd = Math.max(Number(oldId), Number(newId));
                for (let i = lowEnd; i <= highEnd; i++) {
                    highlightLine.push(`${dir}-${i}`);
                }
            }
        }
        this.setState({
            highlightLine,
        });
    };

    private syntaxHighlight = (str: string): any => {
        if (!str) return;
        const language = P.highlight(str, P.languages.javascript);
        return <span dangerouslySetInnerHTML={{ __html: language }} />;
    };

    public render(): JSX.Element {
        return (
            <div className="diff-viewer">
                <div className="diff-header d-flex justify-content-end my-2" >
                    <div className="btn-group btn-group-toggle" data-toggle="buttons" >
                        <label className="btn btn-secondary active" >
                            <input type="radio" name="options" value="split" 
                                checked={this.state.split === true}
                                onChange={this.onSplitChange}/>{this.state.splitText}
                        </label>

                        <label className="btn btn-secondary">
                            <input className="btn btn-secondary" type="radio" name="options" value="unified"
                                checked={this.state.split === false}
                                onChange={this.onSplitChange}/>{this.state.unifiedText}
                        </label>
                    </div>
                </div>
                <ReactDiff
                    highlightLines={this.state.highlightLine}
                    onLineNumberClick={this.onLineNumberClick}
                    oldValue={this.state.old}
                    splitView={this.state.split}
                    newValue={this.state.new}
                    renderContent={this.syntaxHighlight}
                    useDarkTheme={this.state.useDarkMode}
                    leftTitle={this.state.leftText}
                    rightTitle={this.state.newText}
                />
            </div>
        );
    }
}

ReactDOM.render(<DiffViewer />, document.getElementById('diffapp'));
