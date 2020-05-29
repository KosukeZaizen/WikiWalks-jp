import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { actionCreators } from '../store/WikiWalks';
//import { getEnglishDate } from '../common/functions';
//import Head from './Helmet';

class PagesForTheTitles extends Component {
    componentDidMount() {
        // This method is called when the component is first added to the document
        this.fetchData();
    }

    fetchData() {
        //const titleId = this.props.match.params.wordId;
        const titleId = 11;
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
        console.log("render props", this.props);
        const { word, wordId, categories, pages } = this.props.pages;
        //const page = this.props.pages && this.props.pages[0];
        //const publishDate = page && page.publishDate.split("T").shift();
        //const englishDate = publishDate;
        const category = categories && categories[0];
        const description = `This is a list of the Wikipedia pages including ${word}. Please check the list below to know about ${word}!`;
        const arrDesc = description.split(". ");
        const lineChangeDesc = arrDesc.map((d, i) => <span key={i}>{d}{i < arrDesc.length - 1 && ". "}<br /></span>);
        return (
            <div>
                {/*<Head
                    title={title}
                    desc={description}
                />*/}
                <div className="breadcrumbs" itemScope itemType="https://schema.org/BreadcrumbList" style={{ textAlign: "left" }}>
                    <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                        <Link to="/" itemProp="item" style={{ marginRight: "5px", marginLeft: "5px" }}>
                            <span itemProp="name">
                                {"Home"}
                            </span>
                        </Link>
                        <meta itemProp="position" content="1" />
                    </span>
                    {" > "}
                    <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                        <Link to={"/category/" + category} itemProp="item" style={{ marginRight: "5px", marginLeft: "5px" }}>
                            <span itemProp="name">
                                {category}
                            </span>
                            <meta itemProp="position" content="2" />
                        </Link>
                    </span>
                    {" > "}
                    <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                        <span itemProp="name" style={{ marginRight: "5px", marginLeft: "5px" }}>
                            {word}
                        </span>
                        <meta itemProp="position" content="3" />
                    </span>
                </div>
                <hr />
                <h1>{word}</h1>
                <br />
                {lineChangeDesc}
                <br />
                <hr />
                <h2>Pages related to {word}</h2>
                {renderTable(this.props)}
                <hr />
                {categories && categories.length > 0 && categories.map(c => (<div>
                    <h2>Other themes searched on {c}</h2>
                    {renderOtherTable(this.props)}
                    <center>
                        <Link to={`/date/${c}`}><button>Check all themes searched on {c} >></button></Link>
                    </center>
                    <br />
                </div>
                ))}
            </div>
        );
    }
}

function renderTable(props) {
    const { pages } = props;
    return (
        <table className='table table-striped'>
            <thead>
                <tr>
                    <th>Page Title</th>
                    <th>Snippet</th>
                </tr>
            </thead>
            <tbody>
                {pages.length > 0 ? pages.map((page, i) =>
                    <tr key={i}>
                        <td><a href={page.link} target="_blank" rel="noopener noreferrer">{page.pageName}</a></td>
                        <td>{page.explanation}</td>
                    </tr>
                )
                    :
                    <tr><td>Loading...</td><td></td></tr>}
            </tbody>
        </table>
    );
}

function renderOtherTable(props) {
    const titles = props.titles
        .filter(t => props.pages[0] && (t.titleId !== props.pages[0].titleId))
        .filter((t, i) => {
            try {
                const l = 13;
                const n = Math.floor(props.titles.length / l);
                const s = props.pages[0].titleId % n;
                return (i + s) % n === 0;
            } catch (ex) {
                return false;
            }
        });
    return (
        <table className='table table-striped'>
            <thead>
                <tr>
                    <th>Theme</th>
                    <th>Found Articles</th>
                </tr>
            </thead>
            <tbody>
                {titles.length > 0 ? titles.map(title =>
                    <tr key={title.titleId}>
                        <td><Link to={"/theme/" + title.titleId}>{title.title}</Link></td>
                        <td>{title.cnt} articles</td>
                    </tr>
                )
                    :
                    <tr><td>Loading...</td><td></td></tr>}
            </tbody>
        </table>
    );
}

export default connect(
    state => state.wikiWalks,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(PagesForTheTitles);