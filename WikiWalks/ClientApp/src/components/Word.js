import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { actionCreators } from '../store/WikiWalks';
import Head from './Helmet';
import AnchorLink from 'react-anchor-link-smooth-scroll';

class PagesForTheTitles extends Component {
    componentDidMount() {
        // This method is called when the component is first added to the document
        this.fetchData();
    }

    fetchData() {
        const titleId = this.props.match.params.wordId;
        this.props.requestPagesForTheTitle(titleId);

        const page = this.props.pages && this.props.pages[0];
        const publishDate = page && page.publishDate.split("T").shift();

        publishDate && this.props.requestTitlesForTheDate(publishDate.split("-").join(""));
    }

    componentDidUpdate(previousProps) {
        const pagesLoaded = previousProps.pages.length <= 0 && this.props.pages[0] && this.props.pages[0].publishDate;
        const changedTheme = previousProps.location !== this.props.location;
        if (pagesLoaded || changedTheme) {
            this.fetchData();
        }
    }

    render() {
        const { wordId, categories } = this.props.pages;
        const word = this.props.pages.word || "";
        const cat = categories && categories.sort(c => c.cnt)[0];
        const category = cat && cat.category;
        const categoryForUrl = category && category.split(" ").join("_");
        const description = `This is a list of the Wikipedia pages about "${word}". Please check the list below to learn about "${word}"!`;
        const arrDesc = description.split(". ");
        const lineChangeDesc = arrDesc.map((d, i) => <span key={i}>{d}{i < arrDesc.length - 1 && ". "}<br /></span>);
        return (
            <div>
                <Head
                    title={word}
                    desc={description}
                />
                {<div className="breadcrumbs" itemScope itemType="https://schema.org/BreadcrumbList" style={{ textAlign: "left" }}>
                    <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                        <Link to="/" itemProp="item" style={{ marginRight: "5px", marginLeft: "5px" }}>
                            <span itemProp="name">
                                {"Home"}
                            </span>
                        </Link>
                        <meta itemProp="position" content="1" />
                    </span>
                    {" > "}
                    {
                        category ?
                            <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                                <Link to={"/category/" + categoryForUrl} itemProp="item" style={{ marginRight: "5px", marginLeft: "5px" }}>
                                    <span itemProp="name">
                                        {category}
                                    </span>
                                    <meta itemProp="position" content="2" />
                                </Link>
                            </span>
                            :
                            <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                                <Link to={"/all"} itemProp="item" style={{ marginRight: "5px", marginLeft: "5px" }}>
                                    <span itemProp="name">
                                        {"All Keywords"}
                                    </span>
                                    <meta itemProp="position" content="2" />
                                </Link>
                            </span>
                    }
                    {" > "}
                    <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                        <span itemProp="name" style={{ marginRight: "5px", marginLeft: "5px" }}>
                            {word}
                        </span>
                        <meta itemProp="position" content="3" />
                    </span>
                </div>}
                <hr />
                <h1>{word}</h1>
                <br />
                {lineChangeDesc}
                <br />
                <div style={{ maxWidth: "500px", padding: "10px", marginBottom: "10px", border: "5px double gray" }}>
                    <center><p style={{ fontWeight: "bold", fontSize: "large" }}>Index</p></center>
                    <hr />
                    {word ? <ul>
                        <li><AnchorLink href={`#Pages about ${word}`}>{`Pages about ${word}`}</AnchorLink></li>
                        {categories && categories.length > 0 && categories.map((c, i) => (
                            <li key={i}><AnchorLink href={"#" + c.category}>{c.category}</AnchorLink></li>
                        ))}
                    </ul>
                        :
                        <center>Loading...<br /></center>
                    }
                </div>
                <hr />
                <h2 id={`Pages about ${word}`}>{`Pages about ${word}`}</h2>
                {renderTable(this.props)}
                {categories && categories.length > 0 && categories.map((c, i) => (
                    <RenderOtherTable
                        key={i}
                        c={c}
                        wordId={wordId}
                    />
                ))}
            </div>
        );
    }
}

function renderTable(props) {
    const { pages, wordId, word } = props.pages;
    return (
        <table className='table table-striped'>
            <thead>
                <tr>
                    <th>Page Title</th>
                    <th>Snippet</th>
                </tr>
            </thead>
            <tbody>
                {pages && pages.length > 0 ?
                    pages
                        .sort((page1, page2) => page2.snippet.split(word).length - page1.snippet.split(word).length)
                        .map((page, i) => (
                            <tr key={i}>
                                <td>
                                    {page.wordId !== wordId && page.referenceCount > 4 ? <Link to={"/word/" + page.wordId}>{page.word}</Link> : page.word}
                                </td>
                                <td>
                                    {page.snippet.split(" ").map((s, j) => {
                                        const patterns = {
                                            '&lt;': '<',
                                            '&gt;': '>',
                                            '&amp;': '&',
                                            '&quot;': '"',
                                            '&#x27;': '\'',
                                            '&#x60;': '`'
                                        };
                                        Object.keys(patterns).forEach(k => { s = s.split(k).join(patterns[k]) });
                                        const symbol = j === 0 ? "" : " ";
                                        const words = props.pages.word.split(" ");
                                        if (words.some(w => w.toLowerCase() === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}<b>{s}</b></React.Fragment>;
                                        } else if (words.some(w => (w.toLowerCase() + ",") === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}<b>{s.slice(0, -1)}</b>,</React.Fragment>;
                                        } else if (words.some(w => (w.toLowerCase() + ",\"") === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}<b>{s.slice(0, -1)}</b>{",\""}</React.Fragment>;
                                        } else if (words.some(w => (w.toLowerCase() + ".") === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}<b>{s.slice(0, -1)}</b>.</React.Fragment>;
                                        } else if (words.some(w => (w.toLowerCase() + ")") === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}<b>{s.slice(0, -1)}</b>{")"}</React.Fragment>;
                                        } else if (words.some(w => (w.toLowerCase() + "\"") === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}<b>{s.slice(0, -1)}</b>{"\""}</React.Fragment>;
                                        } else if (words.some(w => ("(" + w.toLowerCase()) === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}{"("}<b>{s.substr(1)}</b></React.Fragment>;
                                        } else if (words.some(w => ("\"" + w.toLowerCase()) === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}{"\""}<b>{s.substr(1)}</b></React.Fragment>;
                                        } else if (words.some(w => ("\"\"" + w.toLowerCase()) === s.toLowerCase())) {
                                            return <React.Fragment key={j}>{symbol}{"\"\""}<b>{s.substr(1)}</b></React.Fragment>;
                                        } else {
                                            return <React.Fragment key={j}>{symbol}{s}</React.Fragment>;
                                        }
                                    })}
                                    <br />
                                    <a href={"https://en.wikipedia.org/wiki/" + page.word.split(" ").join("_")} target="_blank" rel="noopener noreferrer">
                                        {"Check the Wikipedia page for " + page.word + " >>"}
                                    </a>
                                </td>
                            </tr>
                        ))
                    :
                    <tr><td>Loading...</td><td></td></tr>}
            </tbody>
        </table>
    );
}

class RenderOtherTable extends Component {

    constructor(props) {
        super(props);

        this.state = {
            pages: {},
        }
    }

    componentDidMount() {
        this.fetchData();
    }

    componentDidUpdate(previousProps) {
        if (previousProps.c !== this.props.c) {
            this.fetchData();
        }
    }

    fetchData = async () => {
        const url = `api/WikiWalks/getWordsForCategory?category=${this.props.c.category}`;
        const response = await fetch(url);
        const pages = await response.json();
        this.setState({ pages });
    }

    render() {
        const { pages } = this.state;
        const { c, wordId } = this.props;
        return (<React.Fragment>
            <hr />
            <h2 id={c.category}>{c.category}</h2>
            <table className='table table-striped'>
                <thead>
                    <tr>
                        <th>Page Title</th>
                        <th>Snippet</th>
                    </tr>
                </thead>
                <tbody>
                    {pages.length > 0 ? pages.map(page =>
                        <tr key={page.wordId}>
                            <td>
                                {(page.wordId !== wordId && page.referenceCount > 4) ? <Link to={"/word/" + page.wordId}>{page.word}</Link> : page.word}
                            </td>
                            <td>
                                {page.snippet}
                                <br />
                                <a href={"https://en.wikipedia.org/wiki/" + page.word.split(" ").join("_")} target="_blank" rel="noopener noreferrer">
                                    {"Check the Wikipedia page for " + page.word + " >>"}
                                </a>
                            </td>
                        </tr>
                    )
                        :
                        <tr><td>Loading...</td><td></td></tr>}
                </tbody>
            </table>
        </React.Fragment>);
    }
}

export default connect(
    state => state.wikiWalks,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(PagesForTheTitles);
