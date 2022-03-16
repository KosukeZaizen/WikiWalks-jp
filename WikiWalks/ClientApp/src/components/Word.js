import React, { Component } from "react";
import AnchorLink from "react-anchor-link-smooth-scroll";
import { connect } from "react-redux";
import { Link } from "react-router-dom";
import { Button } from "reactstrap";
import { bindActionCreators } from "redux";
import { actionCreators } from "../store/WikiWalks";
import GoogleAd from "./GoogleAd";
import Head from "./Helmet";

class PagesForTheTitles extends Component {
    sectionStyle = {
        display: "block",
        borderTop: "1px solid #dcdcdc",
        paddingTop: 12,
        marginTop: 12,
        marginBottom: 30,
    };

    constructor(props) {
        super(props);

        this.state = {
            screenWidth: window.innerWidth,
        };

        let timer;
        window.onresize = () => {
            if (timer > 0) {
                clearTimeout(timer);
            }

            timer = setTimeout(() => {
                this.changeScreenSize();
            }, 100);
        };
    }

    changeScreenSize = () => {
        if (this.state.screenWidth !== window.innerWidth) {
            this.setState({
                screenWidth: window.innerWidth,
            });
        }
    };

    componentDidMount() {
        this.fetchData();
    }

    fetchData() {
        this.props.initialize();
        const wordId = this.props.match.params.wordId.split("#")[0];
        this.props.requestWord(wordId);
        this.props.requestCategoriesForTheTitle(wordId);
        this.props.requestPagesForTheTitle(wordId);
    }

    componentDidUpdate(previousProps) {
        const preLoc = previousProps.location.pathname.split("#")[0];
        const curLoc = this.props.location.pathname.split("#")[0];

        if (preLoc !== curLoc) {
            this.fetchData();
        }
    }

    render() {
        const isWide = this.state.screenWidth > 991;

        const wordId = Number(this.props.match.params.wordId.split("#")[0]);
        const { pages, categories } = this.props;
        const word = this.props.word || "Loading...";
        const cat =
            categories &&
            categories.sort((c1, c2) => c1.cnt - c2.cnt)[categories.length - 1];
        const category = cat && cat.category;
        const categoryForUrl =
            category && encodeURIComponent(category.split(" ").join("_"));
        const description = `「${word}」に関するWikipedia記事の一覧です。「${word}」の話題に触れているページや、「${word}」と関連が深いページをご紹介します。`;
        const arrDesc = description.split("。");
        const lineChangeDesc = arrDesc.map((d, i) => (
            <span key={i}>
                {d}
                {i < arrDesc.length - 1 && "。"}
                <br />
            </span>
        ));

        const showAd = false; // Disable Google adsense
        // const showAd = pages && pages.length > 50;

        return (
            <div>
                <Head title={word} desc={description} />
                <div
                    className="breadcrumbs"
                    itemScope
                    itemType="https://schema.org/BreadcrumbList"
                    style={{ textAlign: "left" }}
                >
                    <span
                        itemProp="itemListElement"
                        itemScope
                        itemType="http://schema.org/ListItem"
                    >
                        <Link
                            to="/"
                            itemProp="item"
                            style={{ marginRight: "5px", marginLeft: "5px" }}
                        >
                            <span itemProp="name">{"Home"}</span>
                        </Link>
                        <meta itemProp="position" content="1" />
                    </span>
                    {" > "}
                    {category ? (
                        <span
                            itemProp="itemListElement"
                            itemScope
                            itemType="http://schema.org/ListItem"
                        >
                            <Link
                                to={"/category/" + categoryForUrl}
                                itemProp="item"
                                style={{
                                    marginRight: "5px",
                                    marginLeft: "5px",
                                }}
                            >
                                <span itemProp="name">{category}</span>
                                <meta itemProp="position" content="2" />
                            </Link>
                        </span>
                    ) : (
                        word && (
                            <span
                                itemProp="itemListElement"
                                itemScope
                                itemType="http://schema.org/ListItem"
                            >
                                <Link
                                    to={"/all"}
                                    itemProp="item"
                                    style={{
                                        marginRight: "5px",
                                        marginLeft: "5px",
                                    }}
                                >
                                    <span itemProp="name">
                                        {"キーワード一覧"}
                                    </span>
                                    <meta itemProp="position" content="2" />
                                </Link>
                            </span>
                        )
                    )}
                    {" > "}
                    <span
                        itemProp="itemListElement"
                        itemScope
                        itemType="http://schema.org/ListItem"
                    >
                        <span
                            itemProp="name"
                            style={{ marginRight: "5px", marginLeft: "5px" }}
                        >
                            {word}
                        </span>
                        <meta itemProp="position" content="3" />
                    </span>
                </div>
                <article style={this.sectionStyle}>
                    <h1>{word}</h1>
                    <br />
                    {lineChangeDesc}
                    <span id="indexOfVocabLists"></span>
                    <div style={isWide ? { display: "flex" } : {}}>
                        {categories && categories.length > 0 && (
                            <div
                                style={{
                                    maxWidth: 500,
                                    padding: 10,
                                    marginBottom: isWide ? 20 : 30,
                                    marginRight: isWide ? 20 : 0,
                                    border: "5px double gray",
                                    width: "100%",
                                }}
                            >
                                <center>
                                    <p
                                        style={{
                                            fontWeight: "bold",
                                            fontSize: "large",
                                        }}
                                    >
                                        目次
                                    </p>
                                </center>
                                {word ? (
                                    <ul
                                        style={{
                                            ...this.sectionStyle,
                                            marginBottom: 0,
                                        }}
                                    >
                                        <li>
                                            <AnchorLink
                                                href={`#Pages about ${word}`}
                                            >{`「${word}」に関する記事の一覧`}</AnchorLink>
                                        </li>
                                        {categories.map((c, i) => (
                                            <li key={i}>
                                                <AnchorLink
                                                    href={"#" + c.category}
                                                >
                                                    {c.category}
                                                </AnchorLink>
                                            </li>
                                        ))}
                                    </ul>
                                ) : (
                                    <center>
                                        Loading...
                                        <br />
                                    </center>
                                )}
                            </div>
                        )}
                        {showAd && (
                            <GoogleAd
                                style={{
                                    width: "100%",
                                    marginBottom: isWide ? 0 : 30,
                                }}
                            />
                        )}
                    </div>
                    <section style={this.sectionStyle}>
                        <h2
                            id={`Pages about ${word}`}
                        >{`「${word}」に関する記事の一覧`}</h2>
                        {renderTable(pages, wordId, word)}
                    </section>
                    {showAd && <GoogleAd />}
                    {categories &&
                        categories.length > 0 &&
                        categories.map(c => (
                            <RenderOtherTable
                                key={c.category}
                                c={c}
                                wordId={wordId}
                                sectionStyle={this.sectionStyle}
                                pagesLoaded={pages && pages.length > 1}
                            />
                        ))}
                    {categories && categories.length > 0 && (
                        <React.Fragment>
                            <ReturnToIndex
                                refForReturnToIndex={this.refForReturnToIndex}
                                criteriaId={`Pages about ${word}`}
                            />
                            {showAd && categories.length > 3 && <GoogleAd />}
                            <div style={{ height: "50px" }}></div>
                        </React.Fragment>
                    )}
                </article>
            </div>
        );
    }
}

function renderTable(pages, wordId, word) {
    const pageLoaded = pages && pages.length > 0;

    const data =
        pageLoaded &&
        pages
            .sort(
                (page1, page2) =>
                    page2.snippet
                        .split("<bold>")
                        .join("")
                        .split("</bold>")
                        .join("")
                        .split(word).length -
                    page1.snippet
                        .split("<bold>")
                        .join("")
                        .split("</bold>")
                        .join("")
                        .split(word).length
            )
            .sort((page1, page2) => {
                if (page2.wordId === wordId) {
                    return 1;
                } else if (page1.wordId === wordId) {
                    return -1;
                } else {
                    return 0;
                }
            })
            .map(page => (
                <tr key={page.wordId}>
                    <td style={{ fontWeight: "bold", minWidth: 100 }}>
                        {page.wordId !== wordId && page.referenceCount > 4 ? (
                            <Link to={"/word/" + page.wordId}>{page.word}</Link>
                        ) : (
                            page.word
                        )}
                    </td>
                    <td>
                        {page.snippet
                            .replace(new RegExp("<$"), "")
                            .replace(new RegExp("<b$"), "")
                            .replace(new RegExp("<bo$"), "")
                            .replace(new RegExp("<bol$"), "")
                            .replace(new RegExp("<bold$"), "")
                            .replace(new RegExp("</b$"), "")
                            .replace(new RegExp("</bo$"), "")
                            .replace(new RegExp("</bol$"), "")
                            .replace(new RegExp("</bold$"), "")
                            .split("&lt;")
                            .join("<")
                            .split("&gt;")
                            .join(">")
                            .split("&amp;")
                            .join("&")
                            .split("&quot;")
                            .join('"')
                            .split("&#x27;")
                            .join("'")
                            .split("&#x60;")
                            .join("`")
                            .split("<bold>")
                            .map(bo => bo.split("</bold>"))
                            .flat()
                            .map((s, j) => {
                                if (j % 2 === 0) {
                                    return (
                                        <React.Fragment key={j}>
                                            {s}
                                        </React.Fragment>
                                    );
                                } else {
                                    return (
                                        <span
                                            key={j}
                                            style={{ fontWeight: "bold" }}
                                        >
                                            {s}
                                        </span>
                                    );
                                }
                            })}
                        <br />
                        <Button
                            size="sm"
                            color="dark"
                            href={
                                "https://ja.wikipedia.org/wiki/" +
                                page.word.split(" ").join("_")
                            }
                            target="_blank"
                            rel="noopener noreferrer"
                            style={{ marginTop: 7 }}
                        >
                            「{page.word}」の
                            <span style={{ display: "inline-block" }}>
                                Wikipedia
                            </span>
                            <span style={{ display: "inline-block" }}>
                                ページ
                            </span>
                            を
                            <span style={{ display: "inline-block" }}>
                                開く
                            </span>
                        </Button>
                    </td>
                </tr>
            ));

    return (
        <React.Fragment>
            {
                <table
                    className="table table-striped"
                    style={{ wordBreak: "break-all", marginBottom: 0 }}
                >
                    <thead>
                        <tr>
                            <th>タイトル</th>
                            <th>内容</th>
                        </tr>
                    </thead>
                    <tbody>
                        {pageLoaded ? (
                            data.shift()
                        ) : (
                            <tr>
                                <td>Loading...</td>
                                <td></td>
                            </tr>
                        )}
                    </tbody>
                </table>
            }
            {pageLoaded && (
                <React.Fragment>
                    {pages.length > 50 && <AdBetweenTables />}
                    {data.length > 0 && (
                        <table
                            className="table table-striped"
                            style={{ wordBreak: "break-all", marginBottom: 0 }}
                        >
                            <tbody>
                                <tr style={{ display: "none" }}></tr>
                                {data.splice(0, 9)}
                            </tbody>
                        </table>
                    )}
                    {pages.length > 50 && <AdBetweenTables />}
                    {data.length > 0 && (
                        <table
                            className="table table-striped"
                            style={{ wordBreak: "break-all", marginBottom: 0 }}
                        >
                            <tbody>{data.splice(0, 12)}</tbody>
                        </table>
                    )}
                    {pages.length > 50 && <AdBetweenTables />}
                    {data.length > 0 && (
                        <table
                            className="table table-striped"
                            style={{ wordBreak: "break-all", marginBottom: 0 }}
                        >
                            <tbody>{data.splice(0, 12)}</tbody>
                        </table>
                    )}
                    {pages.length > 50 && <AdBetweenTables />}
                    {data.length > 0 && (
                        <table
                            className="table table-striped"
                            style={{ wordBreak: "break-all", marginBottom: 0 }}
                        >
                            <tbody>{data.splice(0, 12)}</tbody>
                        </table>
                    )}
                    {pages.length > 50 && <AdBetweenTables />}
                    {data.length > 0 && (
                        <table
                            className="table table-striped"
                            style={{ wordBreak: "break-all" }}
                        >
                            <tbody>{data}</tbody>
                        </table>
                    )}
                </React.Fragment>
            )}
        </React.Fragment>
    );
}

function AdBetweenTables() {
    return null; // Disable Google adsense

    return (
        <div style={{ padding: "15px 0" }}>
            <p style={{ margin: 0, fontSize: "small", fontWeight: "bold" }}>
                スポンサーリンク
            </p>
            <GoogleAd />
        </div>
    );
}

class RenderOtherTable extends Component {
    constructor(props) {
        super(props);

        this.state = {
            pages: {},
        };
    }

    componentDidMount() {
        this.props.pagesLoaded && this.fetchData();
    }

    componentDidUpdate(previousProps) {
        if (!previousProps.pagesLoaded && this.props.pagesLoaded) {
            this.fetchData();
        }
    }

    fetchData = async () => {
        const url = `api/WikiWalks/getWordsForCategory?category=${encodeURIComponent(
            this.props.c.category
        )}`;
        const response = await fetch(url);
        const pages = await response.json();
        this.setState({ pages });
    };

    render() {
        const { pages } = this.state;
        const { c, wordId } = this.props;
        return (
            <React.Fragment>
                <section style={this.props.sectionStyle}>
                    <h2 id={c.category}>{c.category}</h2>
                    <table
                        className="table table-striped"
                        style={{ wordBreak: "break-all" }}
                    >
                        <thead>
                            <tr>
                                <th style={{ minWidth: 100 }}>タイトル</th>
                                <th>内容</th>
                            </tr>
                        </thead>
                        <tbody>
                            {pages.length > 0 ? (
                                pages.map(page => (
                                    <tr key={page.wordId}>
                                        <td style={{ fontWeight: "bold" }}>
                                            {page.wordId !== wordId &&
                                            page.referenceCount > 4 ? (
                                                <Link
                                                    to={"/word/" + page.wordId}
                                                >
                                                    {page.word}
                                                </Link>
                                            ) : (
                                                page.word
                                            )}
                                        </td>
                                        <td>
                                            {page.snippet
                                                .replace(new RegExp("<$"), "")
                                                .replace(new RegExp("<b$"), "")
                                                .replace(new RegExp("<bo$"), "")
                                                .replace(
                                                    new RegExp("<bol$"),
                                                    ""
                                                )
                                                .replace(
                                                    new RegExp("<bold$"),
                                                    ""
                                                )
                                                .replace(new RegExp("</b$"), "")
                                                .replace(
                                                    new RegExp("</bo$"),
                                                    ""
                                                )
                                                .replace(
                                                    new RegExp("</bol$"),
                                                    ""
                                                )
                                                .replace(
                                                    new RegExp("</bold$"),
                                                    ""
                                                )
                                                .split("&lt;")
                                                .join("<")
                                                .split("&gt;")
                                                .join(">")
                                                .split("&amp;")
                                                .join("&")
                                                .split("&quot;")
                                                .join('"')
                                                .split("&#x27;")
                                                .join("'")
                                                .split("&#x60;")
                                                .join("`")
                                                .split("<bold>")
                                                .join("")
                                                .split("</bold>")
                                                .join("")}
                                            <br />
                                            <Button
                                                size="sm"
                                                color="dark"
                                                href={
                                                    "https://ja.wikipedia.org/wiki/" +
                                                    page.word
                                                        .split(" ")
                                                        .join("_")
                                                }
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                style={{ marginTop: 7 }}
                                            >
                                                「{page.word}」の
                                                <span
                                                    style={{
                                                        display: "inline-block",
                                                    }}
                                                >
                                                    Wikipedia
                                                </span>
                                                <span
                                                    style={{
                                                        display: "inline-block",
                                                    }}
                                                >
                                                    ページ
                                                </span>
                                                を
                                                <span
                                                    style={{
                                                        display: "inline-block",
                                                    }}
                                                >
                                                    開く
                                                </span>
                                            </Button>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td>Loading...</td>
                                    <td></td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </section>
            </React.Fragment>
        );
    }
}

class ReturnToIndex extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            showReturnToIndex: false,
        };

        window.addEventListener("scroll", this.judge);
    }

    componentDidMount() {
        for (let i = 0; i < 5; i++) {
            setTimeout(() => {
                this.judge();
            }, i * 1000);
        }
    }

    componentWillUnmount() {
        window.removeEventListener("scroll", this.judge);
    }

    judge = () => {
        const { criteriaId } = this.props;
        const elem = document.getElementById(criteriaId);
        if (!elem) return;

        const height = window.innerHeight;

        const offsetY = elem.getBoundingClientRect().top + 700;
        const t_position = offsetY - height;

        if (t_position >= 0) {
            // 上側の時
            this.setState({
                showReturnToIndex: false,
            });
        } else {
            // 下側の時
            this.setState({
                showReturnToIndex: true,
            });
        }
    };

    render() {
        const { showReturnToIndex } = this.state;
        return (
            <div
                style={{
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    position: "fixed",
                    bottom: 0,
                    left: 0,
                    zIndex: showReturnToIndex ? 99999900 : 0,
                    width: window.innerWidth,
                    height: "40px",
                    opacity: showReturnToIndex ? 1.0 : 0,
                    transition: "all 2s ease",
                    fontSize: "large",
                    backgroundColor: "#DDD",
                }}
            >
                <AnchorLink href={`#indexOfVocabLists`}>
                    {"▲ 目次に戻る ▲"}
                </AnchorLink>
            </div>
        );
    }
}

export default connect(
    state => state.wikiWalks,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(PagesForTheTitles);
